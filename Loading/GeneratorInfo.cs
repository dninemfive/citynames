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
    internal T Instantiate<T>()
        where T : ISaveableStringGenerator<NgramInfo>
    {
        throw new NotImplementedException();
    }
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
}
