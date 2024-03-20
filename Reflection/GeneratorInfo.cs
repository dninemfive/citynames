using d9.utl;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace citynames;
public class GeneratorInfo
{
    public readonly string Name, BaseFileName;
    public readonly Type Type;
    private GeneratorInfo(Type type, GeneratorAttribute attr)
    {
        Type = type;
        Name = attr.Name;
        BaseFileName = attr.BaseFileName;
    }
    private static GeneratorInfo FromType(Type type)
        => new(type, type.GetCustomAttribute<GeneratorAttribute>()!);
    public string FileNameFor(int contextLength)
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
        object? result = null;
        LoggableAction action = new(delegate
        {
            if (Type.GetMethod(methodName, _staticAndPublic, signature) is MethodInfo mi)
            {
                try
                {
                    result = mi.Invoke(null, args);
                    return true;
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            }
            else
            {
                string sigString = signature.Select(x => x.ReadableString())
                                        .Where(x => x is not null)
                                        .ListNotation(brackets: null);
                return $"{Type.Name} does not implement {methodName}({sigString}).";
            }
        });
        action.InvokeWithMessage($"{methodName}ing {Type.Name} with args {args.Select(x => x.ShortString()).ListNotation()}");
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
            obj = TryInvoke("Build", [typeof(IEnumerable<NgramInfo>), typeof(int)], [ngrams, contextLength]);
            rebuilt = true;
        }
        if (obj is ISaveableStringGenerator<NgramInfo> result)
        {
            if (rebuilt)
                await result.SaveAsync(FileNameFor(contextLength));
            return result;
        }
        else
        {
            throw new ArgumentException($"Could not successfully load or build generator {Type.Name}!");
        }
    }
}