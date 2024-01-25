namespace citynames;
public class WikidataResultItem
{
    public string item { get; set; }
    public string itemLabel { get; set; }
    /// <summary>
    /// Ordered (longitude, latitude), i.e. (x, y)
    /// </summary>
    public string coords { get; set; }
    public string pop { get; set; }
    public (string name, LatLongPair coords) ToData()
    {
        string[] split = coords.Replace("Point(","").Replace(")","").Split(" ");
        return (itemLabel, new(double.Parse(split[1]), double.Parse(split[0])));
    }
}