using System.Net.Http.Json;
using System.Text.Json;

namespace citynames;
internal class ArcGisBiomeQuerier(HttpClient _client)
    : ICachedQuerier<LatLongPair, string>
{
    public HttpClient Client { get; set; } = _client;
    private readonly Cache<LatLongPair, string> _cache = new("biomeCache.json");
    public Cache<LatLongPair, string> Cache => _cache;
    public string BaseUrl { get; private set; } = File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\citynames\arcgis query url.txt");
    private async Task<JsonDocument?> QueryApiFor(LatLongPair coords)
        => await Client.GetFromJsonAsync<JsonDocument>(BaseUrl.Replace("{y}", $"{coords.Latitude}")
                                                              .Replace("{x}", $"{coords.Longitude}"));
    public string? TryParse(JsonDocument? doc)
        => doc?.RootElement.NullablyGetProperty("layers")?
                           .FirstArrayElement()?
                           .NullablyGetProperty("features")?
                           .FirstArrayElement()?
                           .NullablyGetProperty("attributes")?
                           .NullablyGetProperty("BIOME_NAME")?
                           .GetString();
    public async Task<(string? result, bool cacheHit)> GetBiomeAsync(LatLongPair coords)
        => await ((ICachedQuerier<LatLongPair, string>)this).TransformAsync(coords);
    public void SaveCache()
        => _cache.Save();
}