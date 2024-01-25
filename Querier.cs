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
    public static readonly string WikidataQueryUrl = File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\citynames\wikidata query url.txt");
    public const string WikidataQueryResultPath = @"C:\Users\dninemfive\Documents\workspaces\misc\citynames\wikidata query result.json";
    private static readonly HttpClient _client = new();
    public static async IAsyncEnumerable<(string city, string biome)> GetAllCities()
    {
        foreach((string city, double latitude, double longitude) in GetCityData())
        {
            string? biome = await GetBiomeAsync(latitude, longitude);
            if (biome is null)
                continue;
            yield return (city, biome);
        }
    }
    public static IEnumerable<(string city, double latitude, double longitude)> GetCityData(int threshold = 50000, int limit = 10000)
    {
        return JsonSerializer.Deserialize<List<WikidataResultItem>>(File.ReadAllText(WikidataQueryResultPath))!
                             .Select(x => x.ToData());
        /* todo: get the proper permissions or whatever to do this in code
        HttpResponseMessage? response = await _client.GetAsync(WikidataQueryUrl.Replace("{threshold}", $"{threshold}")
                                                                               .Replace("{limit}", $"{limit}"));
        if (response is null)
            yield break;
        yield return (response.PrettyPrint(), 0, 0);
        */
    }
    public static async Task<string?> GetBiomeAsync(double latitude, double longitude)
    {
        HttpResponseMessage? response = await _client.GetAsync(ArcGisQueryUrl.Replace("{y}", $"{latitude}").Replace("{x}", $"{longitude}"));
        Console.WriteLine(response.PrettyPrint());
        return null;
        JsonDocument? doc = await _client.GetFromJsonAsync<JsonDocument>(ArcGisQueryUrl.Replace("{y}", $"{latitude}")
                                                                                       .Replace("{x}", $"{longitude}"));
        if (doc is null)
            return null;
        try
        {
            return doc.RootElement.GetProperty("layers")
                                  .EnumerateArray()
                                  .First()
                                  .GetProperty("features")
                                  .EnumerateArray()
                                  .First()
                                  .GetProperty("attributes")
                                  .GetProperty("BIOME_NAME")
                                  .GetString();
        } catch(Exception e)
        {
            Console.WriteLine($"Could not get biome for ({latitude}, {longitude}). {e.GetType().Name}: {e.Message}");
            Console.WriteLine(doc.PrettyPrint());
            return null;
        }
    }
}
public class WikidataResultItem
{
    public string item { get; set; }
    public string itemLabel { get; set; }
    public string coords { get; set; }
    public string pop { get; set; }
    public (string name, double latitude, double longitude) ToData()
    {
        string[] split = coords.Replace("Point(","").Replace(")","").Split(" ");
        return (itemLabel, double.Parse(split[0]), double.Parse(split[1]));
    }
}