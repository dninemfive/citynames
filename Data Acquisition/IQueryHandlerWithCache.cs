using System.Net.Http.Json;
using System.Text.Json;

namespace citynames;
public interface IQueryHandlerWithCache<T, U>
    where T : IDictionaryable
{
    protected HttpClient Client { get; }
    protected Cache<T, U> Cache { get; }
    public string BaseUrl { get; }
    public async Task<JsonDocument?> QueryApiFor(T query)
        => await Client.GetFromJsonAsync<JsonDocument>(BaseUrl.ReplaceUsing(query.ToDictionary()));
    public U? TryParse(JsonDocument? doc);
    public async Task<(U? item, bool cacheHit)> TransformAsync(T query)
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
public interface IDictionaryable
{
    public IReadOnlyDictionary<string, object?> ToDictionary();
}