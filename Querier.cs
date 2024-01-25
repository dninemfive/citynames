using d9.utl;
using System.Net.Http.Json;
using System.Text.Json;

namespace citynames;
public static class Querier
{
    public static readonly string ArcGisQueryUrl = File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\citynames\arcgis query url.txt");
    public static readonly string WikidataQueryUrl = File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\citynames\wikidata query url.txt");
    public const string WikidataQueryResultPath = @"C:\Users\dninemfive\Documents\workspaces\misc\citynames\wikidata query result.json";
    public const string BiomeCacheFilename = "biomeCache.json";
    private static readonly HttpClient _client = new();
    public static async IAsyncEnumerable<(string city, string biome)> GetAllCities()
    {
        int ct = 0;
        foreach((string city, LatLongPair coords) in GetCityData())
        {
            string? biome = await GetBiomeAsync(coords);
            Console.Write($"{++ct,4}\t");
            if (biome is null)
            {
                Console.WriteLine($"Could not find biome for {coords} ({city})!");
                continue;
            } 
            else
            {
                Console.WriteLine($"{city,-32}\t{coords.TableString,-24}\t{biome}");
            }
            yield return (city, biome);
            if(ct % 1000 == 0) 
                SaveCache();
        }
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
    private static Dictionary<LatLongPair, string>? _biomeCache = null;
    public static async Task<string?> GetBiomeAsync(LatLongPair coords)
    {
        if (_biomeCache is null)
            LoadCache();
        if (_biomeCache!.TryGetValue(coords, out string? biome))
            return biome;
        JsonDocument? doc = await _client.GetFromJsonAsync<JsonDocument>(ArcGisQueryUrl.Replace("{y}", $"{coords.Latitude}")
                                                                                       .Replace("{x}", $"{coords.Longitude}"));
        if (doc is null)
            return null;
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
            return null;
        }
        return _biomeCache[coords];
    }
    private static List<KeyValuePair<LatLongPair, string>> BiomeCacheTranslationLayer
    {
        get => _biomeCache.ToList();
        set => _biomeCache = value.ToDictionary(x => x.Key, x => x.Value);
    }
    public static void LoadCache()
    {
        if (File.Exists(BiomeCacheFilename))
        {
            BiomeCacheTranslationLayer = JsonSerializer.Deserialize<List<KeyValuePair<LatLongPair, string>>>(File.ReadAllText(BiomeCacheFilename))!;
        }
        else
        {
            _biomeCache = new();
        }
    }
    public static void SaveCache()
    {
        // amazing https://stackoverflow.com/a/56351540
        Console.Write($"Saving cache...");
        File.WriteAllText(BiomeCacheFilename, JsonSerializer.Serialize(BiomeCacheTranslationLayer, new JsonSerializerOptions() { WriteIndented = true }));
        Console.WriteLine($"...Done!");
    }
}