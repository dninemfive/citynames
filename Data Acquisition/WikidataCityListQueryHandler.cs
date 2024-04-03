using System.Text.Json;

namespace citynames;
/// <summary>
/// In theory, wrapper for queries to Wikidata for the cities and their coordinates used to find
/// the biomes associated with them. As currently implemented, just loads a JSON of manually-saved
/// results from said query.
/// </summary>
/// <param name="client">The HTTP client with which to make queries.</param>
public class WikidataCityListQueryHandler(HttpClient client)
{
    public static readonly string QueryUrl 
        = File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\citynames\wikidata query url.txt");
    public const string QueryResultPath 
        = @"C:\Users\dninemfive\Documents\workspaces\misc\citynames\wikidata query result.json";
    public HttpClient Client { get; private set; } = client;
    /// <summary>
    /// Gets the cities from the query
    /// </summary>
    /// <param name="minPopulation">Currently unused.</param>
    /// <param name="limit">Currently unused.</param>
    /// <returns>An enumerable of the city names in the dataset and their respective geospatial
    ///          coordinates.</returns>
    public IEnumerable<CityCoordPair> GetCityData(int minPopulation = 50000, int limit = 10000)
    {
        return JsonSerializer.Deserialize<List<WikidataResultItem>>(File.ReadAllText(QueryResultPath))!
                             .Select(x => x.ToData())
                             .Distinct()
                             .OrderBy(x => x.Name);
        /* todo: get the proper permissions or whatever to do this in code
        HttpResponseMessage? response = await Client.GetAsync(WikidataQueryUrl.Replace("{threshold}", $"{threshold}")
                                                                               .Replace("{limit}", $"{limit}"));
        if (response is null)
            yield break;
        yield return (response.PrettyPrint(), 0, 0);
        */
    }
}