using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace citynames;
/// <summary>
/// A wrapper for a dictionary which can be saved to a file and which returns information about
/// whether specific queries were found in the cache.
/// </summary>
/// <typeparam name="K">The type of the cache's keys.</typeparam>
/// <typeparam name="V">The type of the cache's values.</typeparam>
/// <param name="filename">The path to the file where this cache will save its data.</param>
public class Cache<K, V>(string filename)
    : IEnumerable<KeyValuePair<K, V>>
    where K : notnull
{
    private static readonly JsonSerializerOptions _indented = new() { WriteIndented = true };
    private Dictionary<K, V>? _dict = null;
    private Dictionary<K, V> Dict => _dict ?? throw NotLoaded;
    // https://stackoverflow.com/a/74170483
    /// <summary><inheritdoc cref="Cache{K, V}" path="/param[@name='filename']/node()"/></summary>
    public string Filename { get; private set; } = filename;
    private InvalidOperationException NotLoaded => new($"Attempted to access {this} before it was initialized.");
    /// <summary>
    /// Gets or sets the item with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key of the item to get or set.</param>
    /// <returns>The value with the specified key, if applicable.</returns>
    public V this[K key]
    {
        get => Dict[key];
        set => Dict[key] = value;
    }
    /// <summary>
    /// Tries to get an item with the specified <paramref name="key"/>, and returns it if found.
    /// </summary>
    /// <param name="key">The key of the item to get.</param>
    /// <param name="value">The value with the specified <paramref name="key"/>, if present;
    ///                     <see langword="null"/> otherwise.</param>
    /// <returns><see langword="true"/> if an item with the specified key was found, 
    ///          or <see langword="false"/> otherwise.</returns>
    public bool TryGetValue(K key, [NotNullWhen(true)] out V? value)
        /* "Parameter must have a non-null value when returning true": Dictionary<K,V> implements this,
         * and i'm fairly sure the annotation is not propagating properly because of the null-coalescing operator.
         */
#pragma warning disable CS8762
        => Dict.TryGetValue(key, out value);
#pragma warning restore CS8762

    #region serialization
    /// <summary>
    /// Ensures that the cache has loaded from its file, if it exists.
    /// </summary>
    public void EnsureLoaded()
    {
        if (_dict is null)
            Load();
    }
    /// <summary>
    /// Loads the cache from its associated file. If no file is found, initializes an empty cache.
    /// </summary>
    public void Load()
    {
        LoggableAction action = new(delegate
        {
            bool exists = File.Exists(Filename);
            TranslationLayer = exists ? JsonSerializer.Deserialize<List<KeyValuePair<K, V>>>(File.ReadAllText(Filename))! : new();
            return new(exists, "file not found");
        });
        action.InvokeWithMessage($"Loading {this.ReadableTypeString()} from `{Filename}`");
    }
    /// <summary>
    /// Saves the cache to its associated file.
    /// </summary>
    public void Save()
    {
        LoggableAction action = new(delegate
        {
            bool result = _dict is not null;
            if(result)
                File.WriteAllText(Filename, JsonSerializer.Serialize(TranslationLayer, _indented));
            return new(result, "cache is null");
        });
        action.InvokeWithMessage($"Saving {this.ShortString()} to `{Filename}`");
    }
    /// <summary>
    /// Converts the internal dictionary to and from a list for serialization purposes.
    /// </summary>
    /// <remarks>Used because <see cref="LatLongPair"/> wasn't working as a key; see 
    ///          <see href="https://stackoverflow.com/a/56351540">this StackOverflow answer</see>
    ///          for details.</remarks>
    private List<KeyValuePair<K, V>> TranslationLayer
    {
        get => _dict!.ToList();
        set => _dict = value.ToDictionary(x => x.Key, x => x.Value);
    }
    #endregion serialization

    public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        => ((IEnumerable<KeyValuePair<K, V>>)Dict).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable)Dict).GetEnumerator();
}