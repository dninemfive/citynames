﻿namespace citynames;
/// <summary>
/// Loads and integrates all the data used by the string generators.
/// </summary>
public static class DataLoader
{
    private static HttpClient? _client = null;
    /// <summary>
    /// The client used to do all the requisite queries.
    /// </summary>
    public static HttpClient Client
    {
        get
        {
            if (_client is null)
            {
                _client = new();
                _client.DefaultRequestHeaders.Add("User-Agent", "citynames/0.0 dninemfive");
            }
            return _client;
        }
    }
    private static readonly HashSet<string> _biomeCache = new();
    /// <summary>
    /// Every unique biome string seen during loading.
    /// </summary>
    public static IReadOnlySet<string> AllBiomes => _biomeCache;
    private static readonly ArcGisBiomeQueryHandler _arcGisQuerier = new(Client);
    private static readonly WikidataCityListQueryHandler _wikidataQuerier = new(Client);
    /// <summary>
    /// Gets all the cities in the wikidata dataset and their corresponding biomes.
    /// </summary>
    /// <returns>An enumerable of said data.</returns>
    public static IEnumerable<(string city, string biome)> GetAllCityData()
        => GetAllCityDataAsync().ToBlockingEnumerable();
    /// <summary><inheritdoc cref="GetAllCityData" path="/summary"/></summary>
    /// <param name="print">If <see langword="true"/>, prints information as the data is loaded.</param>
    /// <returns><inheritdoc cref="GetAllCityData" path="/returns"/></returns>
    public static async IAsyncEnumerable<(string city, string biome)> GetAllCityDataAsync(bool print = false)
    {
        void printProgress(object? item)
        {
            if (print)
                Console.WriteLine(item);
        }
        printProgress("GetAllCityDataAsync()");
        int ct = 0;
        foreach ((string city, LatLongPair coords) in _wikidataQuerier.GetCityData())
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
                printProgress($"{city,-32}\t{coords,-24}\t{biome}");
            }
            _biomeCache.Add(biome);
            yield return (city, biome);
        }
        _arcGisQuerier.SaveCache();
    }
}