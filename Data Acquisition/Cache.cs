using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace citynames;
internal class Cache<K, V>(string filename)
    where K : notnull
{
    private static readonly JsonSerializerOptions _indented = new() { WriteIndented = true };
    private Dictionary<K, V>? _dict = null;
    public string Filename { get; private set; } = filename;
    private InvalidOperationException _notLoaded => new($"Attempted to access {this} before it was initialized.");
    public V this[K key]
    {
        get => (_dict ?? throw _notLoaded)[key];
        set => (_dict ?? throw _notLoaded)[key] = value;
    }
    public bool TryGetValue(K key, [NotNullWhen(true)] out V? value)
#pragma warning disable CS8762 // Parameter must have a non-null value when returning true: Dictionary<K,V> implements this,
                               // and i'm fairly sure the annotation is not propagating properly because of the null-coalescing operator.
        => (_dict ?? throw _notLoaded).TryGetValue(key, out value);
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
        action.InvokeWithMessage($"Loading {this} from `{Filename}`");
    }
    public void Save()
    {
        LoggableAction action = new(delegate
        {
            bool result = _dict is not null;
            if(result) File.WriteAllText(Filename, JsonSerializer.Serialize(TranslationLayer, _indented));
            return new(result, "cache is null");
        });
        action.InvokeWithMessage($"Saving {this} to `{Filename}`");
    }
    // used because LatLongPair wasn't working as a key
    // see https://stackoverflow.com/a/56351540
    private List<KeyValuePair<K, V>> TranslationLayer
    {
        get => _dict!.ToList();
        set => _dict = value.ToDictionary(x => x.Key, x => x.Value);
    }
    #endregion serialization
    public override string ToString()
        => $"Cache<{typeof(K).Name}, {typeof(V).Name}>";
}
