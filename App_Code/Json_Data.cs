using System.Collections.ObjectModel;
public class YPoint
{
    public string type = "Point";
    public double[] coordinates { get; set; }
    public YPoint(double Coord1, double Coord2)
    {
        coordinates = new double[2] { Coord1, Coord2 };
    }
}
public class YAdres
{
    public string type = "Feature";
    public int id { get; set; }
    public YPoint geometry { get; set; }
    public YAdres(int idAdres, double Coord1, double Coord2)
    {
        id = idAdres;
        geometry = new YPoint(Coord1, Coord2);
    }
}
public class YCollectionAdres
{
    public string type = "FeatureCollection";
    public Collection<YAdres> features  = new Collection<YAdres>();
}