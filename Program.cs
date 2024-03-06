using citynames;
using d9.utl;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
namespace citynames;
public class Program
{
    public const string OUTPUT_DIRECTORY = "output";
    
    private static void PrintPreview(IDataView dataView, int maxRows = 100)
    {
        DataDebuggerPreview preview = dataView.Preview(maxRows);
        Console.WriteLine($"{maxRows}\t{preview.ColumnView.Select(x => x.Column.Name).Aggregate((x, y) => $"{x}\t{y}")}");
        int ct = 0;
        foreach(DataDebuggerPreview.RowInfo row in preview.RowView)
        {
            Console.Write($"{ct++}");
            foreach (object value in row.Values.Select(x => x.Value))
            {
                if(value is IEnumerable enumerable)
                {
                    foreach(object item in enumerable)
                    {
                        Console.Write($"\t{item}");
                    }
                }
                else
                {
                    Console.Write($"\t{value}");
                } 
            }
            Console.WriteLine();
            if (ct > maxRows)
                break;
        }
    }
    internal class GeneratorInfo(Type type, GeneratorAttribute attr)
    {
        internal string Name = attr.Name, BaseFileName = attr.BaseFileName;
        internal Type Type = type;
        internal string FileNameFor(int contextLength)
            => BaseFileName.Replace("{contextLength}", $"{contextLength}");
        internal T Instantiate<T>()
            where T : ISaveableStringGenerator<NgramInfo>
        {
            throw new NotImplementedException();
        }
    }
    private static async Task Main()
    {
        // DataProcessor.WriteCsv();
        // PrintPreview(dataView, 250);
        // IDataView predictions = model.Transform(dataView);
        //MulticlassClassificationMetrics metrics = mlContext.MulticlassClassification.Evaluate(predictions);
        //Console.WriteLine(metrics.PrettyPrint());
        int contextLength = CommandLineArgs.TryParseValue<int>(nameof(contextLength)) ?? 2;
        string generatorName = CommandLineArgs.TryGet("generator", CommandLineArgs.Parsers.FirstNonNullOrEmptyString) ?? "Markov";
        Dictionary<string, GeneratorInfo> generators = ReflectionUtils.AllLoadedTypesWithAttribute<GeneratorAttribute>()
                                                                                             .Select(x => new GeneratorInfo(x, x.GetCustomAttribute<GeneratorAttribute>()!))
                                                                                             .ToDictionary(x => x.Name);
        ArgumentException invalidGeneratorTypeException = new($"--generator argument must be {generators.Values.NaturalLanguageList()}, not {generatorName}!");
        if (!generators.TryGetValue(generatorName, out GeneratorInfo generatorInfo))
            throw invalidGeneratorTypeException;
        string generatorFilename = generatorInfo.FileNameFor(contextLength);
        bool buildGenerator = CommandLineArgs.GetFlag("rebuild") || !File.Exists(generatorFilename);

        BuildOrLoadInfo bli = buildGenerator ? new(Querier.GetAllCityDataAsync()
                                                          .ToBlockingEnumerable()
                                                          .ToNgrams(contextLength), contextLength)
                                             : new(generatorFilename, contextLength);
        ISaveableStringGenerator<NgramInfo> generator = BuildOrLoadGenerator(generators[generatorName].type, bli);
        await Querier.SaveCache()
                     .WithMessage("Saving cache");
        if (buildGenerator)
            await generator.SaveAsync(generatorFilename)
                           .WithMessage("Saving generators");
        _ = Directory.CreateDirectory(Path.Join(OUTPUT_DIRECTORY, generatorName));

        int numPerBiome   = CommandLineArgs.TryParseValue<int>(nameof(numPerBiome))   ?? 10,
            minCityLength = CommandLineArgs.TryParseValue<int>(nameof(minCityLength)) ??  5,
            maxCityLength = CommandLineArgs.TryParseValue<int>(nameof(maxCityLength)) ?? 40;
        File.WriteAllText("test.txt","");
        using FileStream fs = File.OpenWrite("test.txt");
        using StreamWriter sw = new(fs);
        NgramInfo query = new("Be", "", "Temperate Broadleaf & Mixed Forests");
        if (generator is MulticlassStringGenerator mc)
        {
            // Console.WriteLine(mc.KeyValueMapper.OrderBy(x => x.Key).Select(x => $"\n{x.Key}\t{x.Value}").ListNotation());
            // Console.WriteLine(mc.KeyValueMapper.Values.Count(x => mc.KeyValueMapper.Values.Count(y => x == y) > 1));
            CharacterPrediction prediction = mc.Predict(query);
            Console.WriteLine($"Total weight: {prediction.CharacterWeights.Sum()}");
            Console.WriteLine($"Weight Count: {prediction.CharacterWeights.Length}");
            Console.WriteLine($"Id Count:     {mc.KeyValueMapper.Keys.Count()}");
            float max = prediction.CharacterWeights.Max();
            foreach ((int id, string character) in mc.KeyValueMapper.OrderBy(x => x.Key))
            {
                sw.Write($"{id}\t");
                if (id < prediction.CharacterWeights.Length)
                    sw.Write($"{prediction.CharacterWeights[id] / max:P2}");
                sw.WriteLine($"\t{character}");
            }
        }
        if (generator is MarkovSetStringGenerator ms)
        {
            Console.WriteLine(ms.Alphabet.Count);
        }
        for (int i = 0; i < 10; i++)
            Console.WriteLine(generator.RandomString(query, 5, 20));
        ISaveableStringGenerator<NgramInfo> generator2 = BuildOrLoadGenerator<MarkovSetStringGenerator>(new($"generators_{contextLength}.json", contextLength));
        for (int i = 0; i < 10; i++)
            Console.WriteLine(generator2.RandomString(query, 5, 20));
        return;
        foreach (string biome in DataProcessor.BiomeCache.Order())
        {
            Console.WriteLine(biome);
            string path = Path.Join(OUTPUT_DIRECTORY, generatorName, $"{biome.Replace("/", ",")}.txt");
            path.CreateIfNotExists();
            foreach (string name in generator.RandomStringsOfLength(NgramInfo.Query(biome), numPerBiome, minCityLength, maxCityLength))
                Utils.PrintAndWrite(path, name);
        }
    }
    public class BuildOrLoadInfo
    {
        public bool Build => Path is null && (Ngrams ?? throw new Exception()) is not null;
        public string? Path { get; private set; } = null;
        public IEnumerable<NgramInfo>? Ngrams { get; private set; } = null;
        public int ContextLength { get; private set; }
        private BuildOrLoadInfo(int contextLength)
            => ContextLength = contextLength;
        public BuildOrLoadInfo(string path, int contextLength = 2) : this(contextLength)
            => Path = path;
        public BuildOrLoadInfo(IEnumerable<NgramInfo> ngrams, int contextLength = 2) : this(contextLength)
            => Ngrams = ngrams;
    }
    public static ISaveableStringGenerator<NgramInfo> BuildOrLoadGenerator<T>(BuildOrLoadInfo bli)
        where T : IBuildLoadableStringGenerator<NgramInfo, T>
    {
        Console.WriteLine($"{(bli.Build ? "Buil" : "Loa")}ding generator...");
        ISaveableStringGenerator<NgramInfo> result = bli.Build ? T.Build(bli.Ngrams!, bli.ContextLength)
                                                               : T.Load(bli.Path!);
        Console.WriteLine("Done.");
        return result;
    }
    private static readonly BindingFlags _staticAndPublic = BindingFlags.Static | BindingFlags.Public;
    public static ISaveableStringGenerator<NgramInfo> BuildOrLoadGenerator(Type t, BuildOrLoadInfo bli)
    {
        Console.WriteLine($"{(bli.Build ? "Buil" : "Loa")}ding generator...");
        object? obj = bli.Build ? t.InvokeMember("Build", _staticAndPublic, null, null, new object?[] { bli.Ngrams!, bli.ContextLength })
                                : t.InvokeMember("Load", _staticAndPublic, null, null, new object?[] { bli.Path! });
        if(obj is ISaveableStringGenerator<NgramInfo> result)
        {
            Console.WriteLine("Done.");
            return result;
        }
        else
        {
            Console.WriteLine("Failed!");
            throw new ArgumentException($"{t.Name} does not implement IBuildLoadableStringGenerator!");
        }
    }
}