﻿using citynames;
using d9.utl;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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
    private static async Task Main()
    {
        //DataProcessor.WriteCsv();
        // PrintPreview(dataView, 250);
        // IDataView predictions = model.Transform(dataView);
        //MulticlassClassificationMetrics metrics = mlContext.MulticlassClassification.Evaluate(predictions);
        //Console.WriteLine(metrics.PrettyPrint());
        int contextLength = CommandLineArgs.TryParseValue<int>(nameof(contextLength)) ?? 2;
        string generatorType = CommandLineArgs.TryGet("generator", CommandLineArgs.Parsers.FirstNonNullOrEmptyString) ?? "markov";
        ArgumentException invalidGeneratorTypeException = new($"--generator argument must be either \"markov\" or \"multiclass\", not {generatorType}!");
        string generatorFilename = generatorType switch
        {
            "markov" => $"generators_{contextLength}.json",
            "multiclass" => "transformedData.csv",
            _ => throw invalidGeneratorTypeException
        };
        bool buildGenerator = CommandLineArgs.GetFlag("rebuild") || !File.Exists(generatorFilename);

        BuildOrLoadInfo bli = buildGenerator ? new(() => Querier.GetAllCityDataAsync().ToNgramsAsync(contextLength), contextLength)
                                             : new(generatorFilename, contextLength);
        ISaveableStringGenerator<NgramInfo> generator = generatorType switch
        {
            "markov" => await BuildOrLoadGeneratorAsync<MarkovSetStringGenerator>(bli),
            "multiclass" => await BuildOrLoadGeneratorAsync<MulticlassStringGenerator>(bli),
            _ => throw invalidGeneratorTypeException
        };
        await Querier.SaveCache().WithMessage("Saving cache");
        if (buildGenerator)
            await generator.SaveAsync(generatorFilename)
                           .WithMessage("Saving generators");
        _ = Directory.CreateDirectory(Path.Join(OUTPUT_DIRECTORY, generatorType));

        int numPerBiome   = CommandLineArgs.TryParseValue<int>(nameof(numPerBiome))   ?? 10,
            minCityLength = CommandLineArgs.TryParseValue<int>(nameof(minCityLength)) ?? 5,
            maxCityLength = CommandLineArgs.TryParseValue<int>(nameof(maxCityLength)) ?? 40;
        File.WriteAllText("test.txt","");
        using FileStream fs = File.OpenWrite("test.txt");
        using StreamWriter sw = new(fs);
        NgramInfo query = new("Ra", "", "Temperate Broadleaf & Mixed Forests");
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
        ISaveableStringGenerator<NgramInfo> generator2 = await BuildOrLoadGeneratorAsync<MarkovSetStringGenerator>(new($"generators_{contextLength}.json", contextLength));
        for (int i = 0; i < 10; i++)
            Console.WriteLine(generator2.RandomString(query, 5, 20));
        return;
        foreach (string biome in DataProcessor.BiomeCache.Order())
        {
            Console.WriteLine(biome);
            string path = Path.Join(OUTPUT_DIRECTORY, generatorType, $"{biome.Replace("/", ",")}.txt");
            path.CreateIfNotExists();
            foreach (string name in generator.RandomStringsOfLength(NgramInfo.Query(biome), numPerBiome, minCityLength, maxCityLength))
                Utils.PrintAndWrite(path, name);
        }
    }
    public class BuildOrLoadInfo
    {
        public bool Build => Path is null && (LoadingMethod ?? throw new Exception()) is not null;
        public string? Path { get; private set; } = null;
        public Func<IAsyncEnumerable<NgramInfo>>? LoadingMethod { get; private set; } = null;
        public int ContextLength { get; private set; }
        private BuildOrLoadInfo(int contextLength)
            => ContextLength = contextLength;
        public BuildOrLoadInfo(string path, int contextLength = 2) : this(contextLength)
            => Path = path;
        public BuildOrLoadInfo(Func<IAsyncEnumerable<NgramInfo>> func, int contextLength = 2) : this(contextLength)
            => LoadingMethod = func;
    }
    public static async Task<ISaveableStringGenerator<NgramInfo>> BuildOrLoadGeneratorAsync<T>(BuildOrLoadInfo bli)
        where T : IBuildLoadAbleStringGenerator<NgramInfo, T>
        => await (bli.Build ? T.BuildAsync(bli.LoadingMethod!(), bli.ContextLength)
                            : T.LoadAsync(bli.Path!))
                            .WithMessage($"{(bli.Build ? "Buil" : "Loa")}ding generator");
}