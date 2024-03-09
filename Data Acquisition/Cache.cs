using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace citynames;
internal class Cache<K, V>(string filename)
    : IEnumerable<KeyValuePair<K, V>>
    where K : notnull
{
    private static readonly JsonSerializerOptions _indented = new() { WriteIndented = true };
    private Dictionary<K, V>? _dict = null;
    private Dictionary<K, V> Dict => _dict ?? throw _notLoaded;
    public string Filename { get; private set; } = filename;
    private InvalidOperationException _notLoaded => new($"Attempted to access {this} before it was initialized.");
    public V this[K key]
    {
        get => Dict[key];
        set => Dict[key] = value;
    }
    public bool TryGetValue(K key, [NotNullWhen(true)] out V? value)
        /* "Parameter must have a non-null value when returning true": Dictionary<K,V> implements this,
         * and i'm fairly sure the annotation is not propagating properly because of the null-coalescing operator.
         */
#pragma warning disable CS8762
        => Dict.TryGetValue(key, out value);
#pragma warning restore CS8762

    #region serialization
    public void EnsureLoaded()
    {
        if (_dict is null)
            Load();
    }
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
    // used because LatLongPair wasn't working as a key see https://stackoverflow.com/a/56351540
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