using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace citynames;
internal interface ICachedQuerier<T, U>
    where T : IDictionaryable
{
    protected HttpClient Client { get; }
    protected Cache<T, U> Cache { get; }
    public string BaseUrl { get; }
    public async Task<JsonDocument?> QueryApiFor(T query)
        => await Client.GetFromJsonAsync<JsonDocument>(BaseUrl.ReplaceUsing(query.ToDictionary()));
    public U? TryParse(JsonDocument? doc);
    public async Task<(U? biome, bool cacheHit)> TransformAsync(T query)
    {
        Cache.EnsureLoaded();
        if (Cache!.TryGetValue(query, out U? result))
            return (result, true);
        result = TryParse(await QueryApiFor(query));
        if (result is not null)
            Cache[query] = result!;
        return (result, false);
    }
    public void SaveCache()
        => Cache.Save();
}
internal interface IDictionaryable
{
    public IReadOnlyDictionary<string, object?> ToDictionary();
}