using d9.utl;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

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
        => new(type, type.GetCustomAttribute<GeneratorAttribute>()!);
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
    private object? TryInvoke(string methodName, Type[] signature, object?[] args)
    {
        MethodInfo? loadMethod = Type.GetMethod(methodName, _staticAndPublic, signature);
        object? result = null;
        if (loadMethod is MethodInfo mi)
        {
            try
            {
                Console.Write($"Invoking {Type.Name}.{methodName}({args.Select(x => x.ReadableTypeString()).ListNotation(brackets: null)})...");
                result = mi.Invoke(null, args);
                Console.WriteLine("Success!");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed: {e.Message}");
            }
        }
        else
        {
            string sigString = signature.Select(x => x.ReadableString())
                                        .Where(x => x is not null)
                                        .ListNotation(brackets: null);
            Console.WriteLine($"Unable to {methodName.ToLower()} generator {Type.Name} because it does not implement {methodName}({sigString}).");
        }
        return result;
    }
    public async Task<ISaveableStringGenerator<NgramInfo>> Instantiate(int contextLength, Func<IEnumerable<NgramInfo>> ngramFn, bool forceRebuild = false)
    {
        object? obj = null;
        bool rebuilt = false;
        if (!forceRebuild)
            obj = TryInvoke("Load", [typeof(string)], [FileNameFor(contextLength)]);
        if (obj is null)
        {
            List<NgramInfo> ngrams = ngramFn!().ToList();
            Console.WriteLine($"Building generator {Type.Name} from {ngrams.Count} {contextLength}-grams...");
            obj = TryInvoke("Build", [typeof(IEnumerable<NgramInfo>), typeof(int)], [ngrams, contextLength]);
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
            throw new ArgumentException($"Could not successfully load or build generator {Type.Name}!");
        }
    }
}