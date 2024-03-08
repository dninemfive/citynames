using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace citynames;
internal class Cache<K, V>(string filename)
    where K : notnull
{
    private static readonly JsonSerializerOptions _indented = new() { WriteIndented = true };
    private Dictionary<K, V>? _dict = null;
    public string Filename { get; private set; } = filename;
    // used because LatLongPair wasn't working as a key
    // see https://stackoverflow.com/a/56351540
    private List<KeyValuePair<K, V>> TranslationLayer
    {
        get => _dict!.ToList();
        set => _dict = value.ToDictionary(x => x.Key, x => x.Value);
    }
    private delegate string LogThingy();
    private void WithMessage(string initialMsg, LogThingy thingy)
    {
        Console.Write($"{initialMsg}...");
        Console.WriteLine(thingy());
    }
    public void Load()
    {
        Console.Write($"Attempting to load Cache<{typeof(K).Name}, {typeof(V).Name}> from file `{Filename}`...");
        if (File.Exists(Filename))
        {
            TranslationLayer = JsonSerializer.Deserialize<List<KeyValuePair<K, V>>>(File.ReadAllText(Filename))!;
            Console.WriteLine("Done.");
        } 
        else
        {
            _dict = new();
            Console.WriteLine("Failed: file not found!");
        }
    }
    public void Save()
    {
        Console.Write($"Attempting to save Cache<{typeof(K).Name}, {typeof(V).Name}> to file `{Filename}`...");
        if (_dict is not null)
        {
            File.WriteAllText(Filename, JsonSerializer.Serialize(TranslationLayer, _indented));
            Console.WriteLine("Done.");
        }
        else
        {
            Console.WriteLine("Failed: cache is null.");
        } 
    }
}
