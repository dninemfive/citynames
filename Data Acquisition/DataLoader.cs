using System.Net.Http.Json;
using System.Text.Json;

namespace citynames;
public static class DataLoader
{
    public static readonly string WikidataQueryUrl = File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\citynames\wikidata query url.txt");
    public const string WikidataQueryResultPath = @"C:\Users\dninemfive\Documents\workspaces\misc\citynames\wikidata query result.json";
    private static HttpClient? _client = null;
    public static HttpClient Client
    {
        get
        {
            if(_client is null)
            {
                _client = new();
                _client.DefaultRequestHeaders.Add("User-Agent", "github.com/dninemfive/citynames");
            }
            return _client;
        }
    }
    private static readonly ArcGisBiomeQuerier _arcGisQuerier = new(Client);
    public static IEnumerable<(string city, string biome)> GetAllCityData()
        => GetAllCityDataAsync().ToBlockingEnumerable();
    public static async IAsyncEnumerable<(string city, string biome)> GetAllCityDataAsync(bool print = false)
    {
        void printProgress(object? item)
        {
            if (print)
                Console.WriteLine(item);
        }
        printProgress("GetAllCityDataAsync()");
        int ct = 0;
        foreach((string city, LatLongPair coords) in GetCityData())
        {
            (string? biome, bool cacheHit) = await _arcGisQuerier.GetBiomeAsync(coords);
            printProgress($"{++ct,8}\t{(cacheHit ? "" : "MISS"),4}\t");
            if (biome is null)
            {
                printProgress($"Could not find biome for {coords} ({city})!");
                continue;
            } 
            else
            {
                printProgress($"{city,-32}\t{coords.TableString,-24}\t{biome}");
            }
            yield return (city, biome);
        }
        _arcGisQuerier.SaveCache();
    }
    public static IEnumerable<(string city, LatLongPair coords)> GetCityData(int threshold = 50000, int limit = 10000)
    {
        return JsonSerializer.Deserialize<List<WikidataResultItem>>(File.ReadAllText(WikidataQueryResultPath))!
                             .Select(x => x.ToData())
                             .DistinctBy(x => x.name)
                             .OrderBy(x => x.name);
        /* todo: get the proper permissions or whatever to do this in code
        HttpResponseMessage? response = await _client.GetAsync(WikidataQueryUrl.Replace("{threshold}", $"{threshold}")
                                                                               .Replace("{limit}", $"{limit}"));
        if (response is null)
            yield break;
        yield return (response.PrettyPrint(), 0, 0);
        */
    }
}