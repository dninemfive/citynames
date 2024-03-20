﻿using System.Text.Json;

namespace citynames;
public class ArcGisBiomeQueryHandler(HttpClient client)
    : IQueryHandlerWithCache<LatLongPair, string>
{
    public HttpClient Client { get; set; } = client;
    private readonly Cache<LatLongPair, string> _cache = new("biomeCache.json");
    public Cache<LatLongPair, string> Cache => _cache;
    public string BaseUrl { get; private set; } = File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\citynames\arcgis query url.txt");
    public string? TryParse(JsonDocument? doc)
        => doc?.RootElement.NullablyGetProperty("layers")?
                           .FirstArrayElement()?
                           .NullablyGetProperty("features")?
                           .FirstArrayElement()?
                           .NullablyGetProperty("attributes")?
                           .NullablyGetProperty("BIOME_NAME")?
                           .GetString();
    public async Task<(string? result, bool cacheHit)> GetBiomeAsync(LatLongPair coords)
        => await ((IQueryHandlerWithCache<LatLongPair, string>)this).TransformAsync(coords);
    public void SaveCache()
        => _cache.Save();
}