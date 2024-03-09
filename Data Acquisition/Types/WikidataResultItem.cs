using System.Text.Json.Serialization;

namespace citynames;
/// <summary>
/// Wrapper for Wikidata, uh, data to make (de)serializing it cleaner.
/// </summary>
public class WikidataResultItem
{
    /// <summary>
    /// The city's Wikidata identifier.
    /// </summary>
    [JsonPropertyName("item")]
    public string Item { get; set; }
    /// <summary>
    /// The city's Wikidata article name.
    /// </summary>
    [JsonPropertyName("itemLabel")]
    public string Name { get; set; }
    /// <summary>
    /// The city's geographic coordinates, interpreted according to the GPS (wkid 4326) <see
    /// href="https://developers.arcgis.com/documentation/spatial-references/">spatial reference</see>.
    /// </summary>
    /// <remarks>
    /// Note: Coordinates are ordered (longitude, latitude), i.e. (x, y), by Wikidata, so these are
    /// reversed to fit the latitude-longitude ordering used by ArcGIS.
    /// </remarks>
    [JsonPropertyName("coords")]
    public string Coordinates { get; set; }
    /// <summary>
    /// The population of the city in question.
    /// </summary>
    [JsonPropertyName("pop")]
    public string Population { get; set; }
    /// <summary>
    /// Converts the string describing the city's <see cref="Coordinates"/> to a <see
    /// cref="LatLongPair"/> and returns it with the name.
    /// </summary>
    /// <returns>
    /// The city's <see cref="Name"/> and a <see cref="LatLongPair"/> describing its coordinates.
    /// </returns>
    /// <remarks><inheritdoc cref="Coordinates" path="/remarks"/></remarks>
    public (string name, LatLongPair coordinates) ToData()
    {
        string[] split = Coordinates.Replace("Point(","").Replace(")","").Split(" ");
        return (Name, new(double.Parse(split[1]), double.Parse(split[0])));
    }
}