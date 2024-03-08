using System.Net.Http.Json;
using System.Text.Json;

namespace citynames;
public static class DataLoader
{
    public static readonly string ArcGisQueryUrl = File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\citynames\arcgis query url.txt");
    public static readonly string WikidataQueryUrl = File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\citynames\wikidata query url.txt");
    public const string WikidataQueryResultPath = @"C:\Users\dninemfive\Documents\workspaces\misc\citynames\wikidata query result.json";
    private static readonly Cache<LatLongPair, string> _biomeCache = new("biomeCache.json");
    private static readonly HttpClient _client = new();
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
            (string? biome, bool cacheHit) = await GetBiomeAsync(coords);
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
            // if(ct % 100 == 0) 
            //    await SaveCache();
        }
        _biomeCache.Save();
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
    public static async Task<(string? result, bool cacheHit)> GetBiomeAsync(LatLongPair coords)
    {
        _biomeCache.EnsureLoaded();
        if (_biomeCache!.TryGetValue(coords, out string? biome))
            return (biome, true);
        JsonDocument? doc = await _client.GetFromJsonAsync<JsonDocument>(ArcGisQueryUrl.Replace("{y}", $"{coords.Latitude}")
                                                                                       .Replace("{x}", $"{coords.Longitude}"));
        if (doc is null)
            return (null, false);
        try
        {
            string result = doc.RootElement.GetProperty("layers")
                                           .EnumerateArray()
                                           .First()
                                           .GetProperty("features")
                                           .EnumerateArray()
                                           .First()
                                           .GetProperty("attributes")
                                           .GetProperty("BIOME_NAME")
                                           .GetString()!;
            _biomeCache[coords] = result;
        } 
        catch
        {
            // Console.WriteLine($"Could not get biome for {coords}. {e.GetType().Name}: {e.Message}");
            // Console.WriteLine(doc.PrettyPrint());
            return (null, false);
        }
        return (_biomeCache[coords], false);
    }
    public static async Task<(string? result, bool cacheHit)> GetBiomeAsync(double latitude, double longitude)
        => await GetBiomeAsync(new(latitude, longitude));
}