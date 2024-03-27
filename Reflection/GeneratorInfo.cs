using d9.utl;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace citynames;
/// <summary>
/// Defines and automates instantiation of specific string generators marked with 
/// <see cref="GeneratorAttribute"/>.
/// </summary>
public class GeneratorInfo
{
    /// <summary><inheritdoc cref="GeneratorAttribute" path="/param[@name='name']/node()"/></summary>
    public string Name { get; private set; }
    /// <summary><inheritdoc cref="GeneratorAttribute" path="/param[@name='fileName']/node()"/></summary>
    public string BaseFileName { get; private set; }
    /// <summary>
    /// The type of the associated string generator.
    /// </summary>
    public readonly Type Type;
    private GeneratorInfo(Type type, GeneratorAttribute attr)
    {
        Type = type;
        Name = attr.Name;
        BaseFileName = attr.BaseFileName;
    }
    /// <summary>
    /// Gets the <see cref="GeneratorInfo"/> for a specified <paramref name="type"/>, if any.
    /// </summary>
    /// <param name="type">The type whose GeneratorInfo to produce.</param>
    /// <returns>The produced GeneratorInfo.</returns>
    /// <remarks>Because this is <see langword="private"/>, it is assumed that the
    ///          <paramref name="type"/> actually has an associated GeneratorAttribute;
    ///          an exception will be thrown if used improperly.</remarks>
    private static GeneratorInfo FromType(Type type)
        => new(type, type.GetCustomAttribute<GeneratorAttribute>()!);
    /// <summary>
    /// The file name for this type of generator with the given parameters.
    /// </summary>
    /// <param name="contextLength">The context length used to produce this particular generator instance.</param>
    /// <returns>The file name to save this generator to or load the same from.</returns>
    public string FileNameFor(int contextLength)
        => BaseFileName.Replace("{contextLength}", $"{contextLength}");
    private static readonly Dictionary<string, GeneratorInfo> _dict
            = ReflectionUtils.AllLoadedTypesWithAttribute<GeneratorAttribute>()
                             .Select(FromType)
                             .ToDictionary(x => x.Name);
    /// <summary>
    /// Tries to get the <see cref="GeneratorInfo"/> associated with the specified 
    /// <see cref="Name">Name</see>, if it exists.
    /// </summary>
    /// <param name="name">The name of the GeneratorInfo to get.</param>
    /// <param name="info">The GeneratorInfo, if found, or <see langword="null"/> otherwise.</param>
    /// <returns><see langword="true"/> if the info was found, or <see langword="false"/> otherwise.
    /// </returns>
    public static bool TryGetByName(string name, [NotNullWhen(true)] out GeneratorInfo? info)
        => _dict.TryGetValue(name, out info);
    /// <summary>
    /// Gets the <see cref="GeneratorInfo"/> associated with the specified <see cref="Name">Name</see>,
    /// throwing an exception if not found.
    /// </summary>
    /// <param name="name"><inheritdoc
    ///                         cref="TryGetByName(string, out GeneratorInfo?)"
    ///                         path="/param[@name='name']/node()"/>
    /// </param>
    /// <returns>The <see cref="GeneratorInfo"/> with the specified <paramref name="name"/>.</returns>
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
    /// <summary>
    /// Tries to build or load a specified string generator.
    /// </summary>
    /// <param name="contextLength">The context length of the specific generator to build.</param>
    /// <param name="ngramFn">A function which, when called, will provide the required ngrams to build the generator.</param>
    /// <param name="forceRebuild">If <see langword="true"/>, loading will not be attempted and the
    ///        generator will always be built.</param>
    /// <returns>The generator as defined above.</returns>
    /// <exception cref="ArgumentException">Thrown if both loading and building fail, usually because
    ///            the type does not implement the required methods.</exception>
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