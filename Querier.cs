using d9.utl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace citynames;
public static class Querier
{
    public static readonly string ArcGisQueryUrl = File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\citynames\arcgis query url.txt");
    private static HttpClient _client = new();
    public static async IAsyncEnumerable<(string city, double latitude, double longitude)> GetCities()
    {
        throw new NotImplementedException();
        yield break;
    }
    public static async Task<string?> GetBiomeAsync(double latitude, double longitude)
    {
        JsonDocument? doc = await _client.GetFromJsonAsync<JsonDocument>(ArcGisQueryUrl.Replace("{y}", $"{latitude}")
                                                                                       .Replace("{x}", $"{longitude}"));
        if (doc is null)
            return null;
        return doc.RootElement.GetProperty("layers")
                              .EnumerateArray()
                              .First()
                              .GetProperty("features")
                              .EnumerateArray()
                              .First()
                              .GetProperty("attributes")
                              .GetProperty("BIOME_NAME")
                              .GetString();
    }
}
