using System.Net.Http.Json;
using System.Text.Json;

namespace citynames;
/// <summary>
/// Abstraction for the concept of trying to load cached data, if possible, before querying for
/// that data.
/// </summary>
/// <typeparam name="T">The type of the queries to be made.</typeparam>
/// <typeparam name="U">The type of the results of those queries.</typeparam>
public interface IQueryHandlerWithCache<T, U>
    where T : IDictionaryable
{
    protected HttpClient Client { get; }
    protected Cache<T, U> Cache { get; }
    public string BaseUrl { get; }
    public async Task<JsonDocument?> QueryApiFor(T query)
        => await Client.GetFromJsonAsync<JsonDocument>(BaseUrl.ReplaceUsing(query.ToDictionary()));
    public U? TryParse(JsonDocument? doc);
    /// <summary>
    /// Attempts to find the specified <paramref name="query"/> in the cache; if not found,
    /// performs the query and returns the results, if any.
    /// </summary>
    /// <param name="query">The query to perform to get the desired data, if not found in the cache.</param>
    /// <returns>
    /// A tuple where:
    /// <list type="bullet">
    /// <item><c>item</c> is the query result, if found as described above, or 
    ///       <see langword="null"/> otherwise;</item>
    /// <item><c>cacheHit</c> is <see langword="true"/> if the item was found in the cache, or
    ///       <see langword="false"/> otherwise.</item>
    /// </list>
    /// </returns>
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
/// <summary>
/// Specifies that a type can be converted into a dictionary using <see cref="ToDictionary"/>.
/// </summary>
public interface IDictionaryable
{
    /// <summary>
    /// Converts this type into a dictionary.
    /// </summary>
    /// <returns>A weakly-typed dictionary of the relevant members of the implementing type.</returns>
    public IReadOnlyDictionary<string, object?> ToDictionary();
}