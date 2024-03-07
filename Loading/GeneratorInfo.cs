using d9.utl;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace citynames;
internal class GeneratorInfo
{
    internal readonly string Name, BaseFileName;
    internal readonly Type Type;
    private GeneratorInfo(Type type, GeneratorAttribute attr)
    {
        Type = type;
        Name = attr.Name;
        BaseFileName = attr.BaseFileName;
    }
    private static GeneratorInfo FromType(Type type)
        => new (type, type.GetCustomAttribute<GeneratorAttribute>()!);
    internal string FileNameFor(int contextLength)
        => BaseFileName.Replace("{contextLength}", $"{contextLength}");
    private static readonly Dictionary<string, GeneratorInfo> _dict
            = ReflectionUtils.AllLoadedTypesWithAttribute<GeneratorAttribute>()
                             .Select(FromType)
                             .ToDictionary(x => x.Name);
    public static bool TryGetByName(string name, [NotNullWhen(true)] out GeneratorInfo? info)
        => _dict.TryGetValue(name, out info);
    public static GeneratorInfo GetByName(string name)
        => TryGetByName(name, out GeneratorInfo? result) ? result : throw InvalidGeneratorTypeException(name);
    private static ArgumentException InvalidGeneratorTypeException(string name)
        => new($"--generator argument must be {_dict.Keys.Order().NaturalLanguageList()}, not {name}!");
    private static readonly BindingFlags _staticAndPublic = BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod;
    public async Task<ISaveableStringGenerator<NgramInfo>> Instantiate(int contextLength, Func<IEnumerable<NgramInfo>> ngramFn, bool forceRebuild = false)
    {
        object? obj = null;
        bool rebuilt = false;
        if(!forceRebuild)
        {
            try
            {
                string filename = FileNameFor(contextLength);
                Console.WriteLine($"Attempting to load generator {Type.Name} from {filename}...");
                obj = Type.InvokeMember("Load", _staticAndPublic, null, null, [FileNameFor(contextLength)]);
                Console.WriteLine("Success!");
            } 
            catch(Exception e)
            {
                Console.WriteLine($"Failed: {e.Message}");
            }
        }
        if(obj is null)
        {
            List<NgramInfo> ngrams = ngramFn!().ToList();
            Console.WriteLine($"Building generator {Type.Name} from {ngrams.Count} {contextLength}-grams...");
            obj = Type.InvokeMember("Build", _staticAndPublic, null, null, [ngrams, contextLength]);
            rebuilt = true;
        }
        if (obj is ISaveableStringGenerator<NgramInfo> result)
        {
            Console.WriteLine("Done.");
            if (rebuilt)
                await result.SaveAsync(FileNameFor(contextLength));
            return result;
        }
        else
        {
            Console.WriteLine("Failed!");
            throw new ArgumentException($"{Type.Name} does not implement IBuildLoadableStringGenerator!");
        }
    }
}
