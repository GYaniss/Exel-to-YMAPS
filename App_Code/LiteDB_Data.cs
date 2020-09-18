
/// <summary>
/// обьекты для LiteDB
/// </summary>
public class ExelRows
{
    public int Id { get; set; }
    public string[] ColumnValue { get; set; }
    public string Adress{ get; set; }
    public string Coord1 { get; set; }
    public string Coord2 { get; set; }
    public bool isGeoCode { get; set; }
    public string Error { get; set; }
}

public class ExelHeaders
{
    public int Id { get; set; }
    public string HeadName { get; set; }
}

public class AllCount
{
    public int Id { get; set; }
    public int Value { get; set; }
}

public class AdressCashe
{
    public int Id { get; set; }
    public string Adress { get; set; }
    public string Coord1 { get; set; }
    public string Coord2 { get; set; }

}

