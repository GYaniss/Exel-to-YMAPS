<%@ WebHandler Language="C#" Class="DetailInfoHandler" %>

using System.Text;
using System.Web;
using System.Configuration;
using LiteDB;

public class DetailInfoHandler : IHttpHandler {

    public void ProcessRequest (HttpContext context) {
        context.Response.ContentType = "text/plain";
        string FName = context.Request["FN"].Trim();
        string[] Id = context.Request["id"].Split((";").ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
        StringBuilder SB = new StringBuilder();
        if (ExelReader.isFileInDB(FName))
        {
            using (var db = new LiteDatabase(ConfigurationManager.AppSettings["LiteDB"]))
            {
                var Head = db.GetCollection<ExelHeaders>("H" + FName.GetHashCode());
                var Massiv = db.GetCollection<ExelRows>("C" + FName.GetHashCode());

                SB.Append("<div class=\"StyleTable\"><table cellspacing=\"0\">");

                foreach (ExelHeaders H in Head.FindAll())
                {
                    if (H.Id==1)
                    {
                        SB.AppendFormat("<thead><tr><td>{0}</td>", H.HeadName);
                        for (int i = 0; i < Id.Length; i++)
                        {
                            int _id = int.Parse(Id[i]);
                            ExelRows Element = Massiv.FindOne(x => x.Id == _id);
                            SB.AppendFormat("<td>{0}</td>", Element.ColumnValue[H.Id-1]);
                           
                        }
                        SB.AppendFormat("</tr></thead><tbody>");

                    }
                    else
                    {
                        SB.AppendFormat("<tr><td>{0}</td>", H.HeadName);
                        for (int i = 0; i < Id.Length; i++)
                        {
                            int _id = int.Parse(Id[i]);
                            ExelRows Element = Massiv.FindOne(x => x.Id == _id);
                            SB.AppendFormat("<td>{0}</td>", Element.ColumnValue[H.Id-1]);
                        }
                        SB.AppendFormat("</tr>");
                    }
                }
                SB.Append("</tbody></table></div>");
            }
        }
        context.Response.Write(SB.ToString());
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}