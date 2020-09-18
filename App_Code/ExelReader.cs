using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using LiteDB;
using System.Configuration;
using System.IO;

/// <summary>
/// Чтение файла Exel.
/// </summary>
public class ExelReader
{
    public enum TipExel { xsl, xslx, error }
    public TipExel Status { get; set; }
    string FName;

    public ExelReader(string FNamed)
    {
        FName = FNamed;
    }
    public void SaveToLiteDB(int NumPage)
    {
        //Инициализация чтения Exel
        XSSFWorkbook xssfworkbook = null; //книга xlsx (2010+)
        HSSFWorkbook hssfworkbook = null; //книга xls (2007)
        string path = ConfigurationManager.AppSettings["StorageRoot"] + FName;
        string ext = Path.GetExtension(path);
        Status = TipExel.error;
        if (ext == ".xls")
        {
            using (FileStream file = new FileStream(path, System.IO.FileMode.Open, FileAccess.Read))
            {
                hssfworkbook = new HSSFWorkbook(file);
            }
            Status = TipExel.xsl;
        }
        if (ext == ".xlsx")
        {
            using (FileStream file = new FileStream(path, System.IO.FileMode.Open, FileAccess.Read))
            {
                xssfworkbook = new XSSFWorkbook(file);
            }
            Status = TipExel.xslx;
        }
        if (Status == TipExel.error) return;
        //Загрузка данных в БД===========================================================
        using (var db = new LiteDatabase(ConfigurationManager.AppSettings["LiteDB"]))
        {
            var head = db.GetCollection<ExelHeaders>("H" + FName.GetHashCode());
            var col = db.GetCollection<ExelRows>("C" + FName.GetHashCode());
            var AllCount = db.GetCollection<AllCount>("A" + FName.GetHashCode());
            var ADRCashe = db.GetCollection<AdressCashe>("AdresCashe");

            ISheet sheet = null;
            if (Status == TipExel.xsl) { sheet = hssfworkbook.GetSheetAt(NumPage); }
            if (Status == TipExel.xslx) { sheet = xssfworkbook.GetSheetAt(NumPage); }
            System.Collections.IEnumerator rows = sheet.GetRowEnumerator();

            int A = 0;
            while (rows.MoveNext()) { A++;}; rows.Reset(); A = A - 1;
            AllCount ALL = AllCount.FindOne(x => x.Id == 1);
            ALL.Value = A;
            AllCount.Update(ALL);

            int idElement = 0;
            int GeoHeader = -1;
            bool header = true;
            while (rows.MoveNext())
            {
                IRow row = null;
                if (Status == TipExel.xsl) { row = (HSSFRow)rows.Current; }
                if (Status == TipExel.xslx) { row = (XSSFRow)rows.Current; }

                //Обработка заголовка файла:
                if (header)
                {
                    for (int j = 0; j < row.LastCellNum; j++)
                    {
                        var H = new ExelHeaders { HeadName = GetTxtFromCell(row.GetCell(j)) };
                        if (H.HeadName.ToUpper().IndexOf("АДРЕС")>=0) { GeoHeader = j; }
                        head.Insert(H);
                    }
                    header = false;
                    continue;
                }
                //Обработка строк файла:
                if (GeoHeader >= 0)
                {
                    string Adress = null, Error = null, Coord1 = null, Coord2 = null, AdresC = null;
                    bool isGeoCode = false;
                    string[] ColumnText = new string[row.LastCellNum];
                    for (int i = 0; i < row.LastCellNum; i++) { ColumnText[i] = GetTxtFromCell(row.GetCell(i)); }
                    Adress = ColumnText[GeoHeader];
                    if (Adress.Length >= 3)
                    {
                        AdresC = Adress.Trim().Replace(" ", "").ToUpper();
                        if (AdresC.IndexOf(",КВ.")>0) { AdresC = AdresC.Remove(AdresC.IndexOf(",КВ.")); };
                        AdressCashe F_Adr = ADRCashe.FindOne(x => x.Adress == AdresC);
                        if (F_Adr == null)
                        {
                            isGeoCode = GeoCoder.DecodeAdress(Adress, out Coord1, out Coord2, out Error);
                            if (isGeoCode)
                            {
                                var N_Adr = new AdressCashe
                                {
                                    Adress = AdresC,
                                    Coord1 = Coord1,
                                    Coord2 = Coord2
                                };
                                ADRCashe.Insert(N_Adr);
                            }
                        }
                        else
                        {
                            isGeoCode = true;
                            Adress = F_Adr.Adress;
                            Coord1 = F_Adr.Coord1;
                            Coord2 = F_Adr.Coord2;
                        }
                    }
                    else { Error = "Мало букв в адресе"; }
                    var C = new ExelRows
                    {
                        ColumnValue = ColumnText,
                        Adress = Adress,
                        Coord1 = Coord1,
                        Coord2 = Coord2,
                        isGeoCode = isGeoCode,
                        Error = Error
                    };
                    col.Insert(C);
                    idElement++;
                }
            }//Геокодирование и добавление полей результатов:
            ADRCashe.EnsureIndex(x => x.Adress);
            ALL.Value = idElement;
            AllCount.Update(ALL);
        } //Открытие БД LiteDB
    }

    string GetTxtFromCell(ICell cell)
    {
        string R = string.Empty;
        if (cell == null) { return R; }
        if (cell.IsMergedCell) { R = "MERGE"; }
        if (cell.CellType == CellType.Blank) { R = string.Empty; }
        if (cell.CellType == CellType.Boolean) { R = cell.BooleanCellValue.ToString(); }
        if (cell.CellType == CellType.Error) { R = "ERROR"; }
        if (cell.CellType == CellType.Formula)
        {
            switch (cell.CachedFormulaResultType)
            {
                case CellType.Blank: R = string.Empty; break;
                case CellType.String: R = cell.StringCellValue; break;
                case CellType.Boolean: R = cell.BooleanCellValue.ToString(); break;
                case CellType.Numeric:
                    if (HSSFDateUtil.IsCellDateFormatted(cell)) { R = cell.DateCellValue.ToString(); }
                    else { R = cell.NumericCellValue.ToString(); }
                    break;
            }
        }
        if (cell.CellType == CellType.Numeric)
        {
            if (HSSFDateUtil.IsCellDateFormatted(cell)) { R = cell.DateCellValue.ToString(); }
            else { R = cell.NumericCellValue.ToString(); }
        }
        if (cell.CellType == CellType.String) { R = cell.StringCellValue; }
        if (cell.CellType == CellType.Unknown) { R = "Unknown"; }
        return R.ToString();
    }

    public static void DeleteFileFromDB(string FName)
    {
        using (var db = new LiteDatabase(ConfigurationManager.AppSettings["LiteDB"]))
        {
            if (db.CollectionExists("H" + FName.GetHashCode())) { db.DropCollection("H" + FName.GetHashCode()); }
            if (db.CollectionExists("C" + FName.GetHashCode())) { db.DropCollection("C" + FName.GetHashCode()); }
            if (db.CollectionExists("A" + FName.GetHashCode())) { db.DropCollection("A" + FName.GetHashCode()); }
            db.Shrink();
        }
    }
    public static bool isFileInDB(string FName)
    {
        bool R = false;
        using (var db = new LiteDatabase(ConfigurationManager.AppSettings["LiteDB"]))
        {
            if (db.CollectionExists("A" + FName.GetHashCode())) { R = true; }
        }
        return R;
    }


}