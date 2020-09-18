<%@ WebHandler Language="C#" Class="GetPoints" %>
using System.Web;
using System.Web.Script.Serialization;
using System.Configuration;
using LiteDB;
using System;
using System.Threading;

public class GetPoints : IHttpHandler {

    protected Guid _id;
    string FName;
    int Current;
    private readonly JavaScriptSerializer js = new JavaScriptSerializer();
    public string StorageRoot {get { return ConfigurationManager.AppSettings["StorageRoot"]; }}
    public bool IsReusable {get {return true;}}


    public void ProcessRequest(HttpContext context)
    {
        context.Response.AddHeader("Pragma", "no-cache");
        context.Response.AddHeader("Cache-Control", "private, no-cache");
        context.Response.ContentType = "application/json";
        FName = context.Request["FN"];
        Current = Int32.Parse(context.Request["Current"]);
        js.MaxJsonLength = 2097152 * 4;
        YCollectionAdres Map = new YCollectionAdres();
        using (var db = new LiteDatabase(ConfigurationManager.AppSettings["LiteDB"]))
        {

            if (db.CollectionExists("A" + FName.GetHashCode()))
            {
                var All = db.GetCollection<AllCount>("A" + FName.GetHashCode());
                var Massiv = db.GetCollection<ExelRows>("C" + FName.GetHashCode());
                AllCount A = All.FindOne(x => x.Id == 1);
                if (Current < A.Value)
                {
                    var results = Massiv.Find(x => x.Id > Current);
                    foreach (ExelRows X in results)
                    {
                        if (X.isGeoCode)
                        {
                            Map.features.Add(new YAdres(X.Id, double.Parse(X.Coord1.Replace('.', ',')), double.Parse(X.Coord2.Replace('.', ','))));
                        }
                        Current = X.Id;
                    }
                }
                else
                {
                    Current = A.Value;
                }
                context.Response.AddHeader("AllCount", A.Value.ToString());
                context.Response.AddHeader("Current", Current.ToString());
                context.Response.Write(js.Serialize(Map));
            }
            else
            {
                var All = db.GetCollection<AllCount>("A" + FName.GetHashCode());
                AllCount A = new AllCount { Value = 1000000 };
                All.Insert(A);
                context.Response.AddHeader("AllCount", "1000000");
                context.Response.AddHeader("Current", "0");
                context.Response.Write(js.Serialize(Map));
                ThreadStart ts = new ThreadStart(DecodeF);
                Thread th = new Thread(ts);
                th.Start();
            }
        }
    }

    protected void DecodeF()
    {
        ExelReader MyExel = new ExelReader(FName);
        MyExel.SaveToLiteDB(0);
    }
}