using System.Text.Json;

namespace citynames;
internal class WikidataCityListQueryHandler(HttpClient client)
{
    public static readonly string QueryUrl = File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\citynames\wikidata query url.txt");
    public const string QueryResultPath = @"C:\Users\dninemfive\Documents\workspaces\misc\citynames\wikidata query result.json";
    public HttpClient Client { get; private set; } = client;
    public IEnumerable<(string city, LatLongPair coords)> GetCityData(int threshold = 50000, int limit = 10000)
    {
        return JsonSerializer.Deserialize<List<WikidataResultItem>>(File.ReadAllText(QueryResultPath))!
                             .Select(x => x.ToData())
                             // .DistinctBy(x => x.name)
                             .OrderBy(x => x.name);
        /* todo: get the proper permissions or whatever to do this in code
        HttpResponseMessage? response = await Client.GetAsync(WikidataQueryUrl.Replace("{threshold}", $"{threshold}")
                                                                               .Replace("{limit}", $"{limit}"));
        if (response is null)
            yield break;
        yield return (response.PrettyPrint(), 0, 0);
        */
    }
}