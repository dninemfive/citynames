using System.Net.Http.Json;
using System.Text.Json;

namespace citynames;
public static class DataLoader
{
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
    private static readonly HashSet<string> _biomeCache = new();
    public static IReadOnlySet<string> AllBiomes => _biomeCache;
    private static readonly ArcGisBiomeQueryHandler _arcGisQuerier = new(Client);
    private static readonly WikidataCityListQueryHandler _wikidataQuerier = new(Client);
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
        foreach((string city, LatLongPair coords) in _wikidataQuerier.GetCityData())
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
            _biomeCache.Add(biome);
            yield return (city, biome);
        }
        _arcGisQuerier.SaveCache();
    }
}