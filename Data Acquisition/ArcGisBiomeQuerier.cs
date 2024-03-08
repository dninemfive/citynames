using System.Net.Http.Json;
using System.Text.Json;

namespace citynames;
internal class ArcGisBiomeQuerier(HttpClient _client)
{
    private readonly Cache<LatLongPair, string> _cache = new("biomeCache.json");
    internal static string BaseUrl { get; private set; } = File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\citynames\arcgis query url.txt");
    private async Task<JsonDocument?> QueryApiFor(LatLongPair coords)
        => await _client.GetFromJsonAsync<JsonDocument>(BaseUrl.Replace("{y}", $"{coords.Latitude}")
                                                               .Replace("{x}", $"{coords.Longitude}"));
    private static string? BiomeFromQuery(JsonDocument? doc)
        => doc?.RootElement.NullablyGetProperty("layers")?
                           .FirstArrayElement()?
                           .NullablyGetProperty("features")?
                           .FirstArrayElement()?
                           .NullablyGetProperty("attributes")?
                           .NullablyGetProperty("BIOME_NAME")?
                           .GetString();
    public async Task<(string? biome, bool cacheHit)> GetBiomeAsync(LatLongPair coords)
    {
        _cache.EnsureLoaded();
        if (_cache!.TryGetValue(coords, out string? biome))
            return (biome, true);
        biome = BiomeFromQuery(await QueryApiFor(coords));
        if(biome is not null)
            _cache[coords] = biome!;
        return (biome, false);
    }
    public async Task<(string? result, bool cacheHit)> GetBiomeAsync(double latitude, double longitude)
        => await GetBiomeAsync(new(latitude, longitude));
    public void SaveCache()
        => _cache.Save();
}