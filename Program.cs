using citynames;
using d9.utl;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
namespace citynames;
public class Program
{
    public const string OUTPUT_DIRECTORY = "output";    
    public static int ContextLength { get; private set; } 
        = CommandLineArgs.TryParseValue<int>(nameof(ContextLength)) ?? 2;
    public static string GeneratorName { get; private set; } 
        = CommandLineArgs.TryGet("generator", CommandLineArgs.Parsers.FirstNonNullOrEmptyString) ?? "Markov";
    private static async Task Main()
    {
        // DataProcessor.WriteCsv();
        // PrintPreview(dataView, 250);
        // IDataView predictions = model.Transform(dataView);
        //MulticlassClassificationMetrics metrics = mlContext.MulticlassClassification.Evaluate(predictions);
        //Console.WriteLine(metrics.PrettyPrint());
        GeneratorInfo generatorInfo = GeneratorInfo.GetByName(GeneratorName);
        ISaveableStringGenerator<NgramInfo> generator = await generatorInfo.Instantiate(ContextLength,
                                                                                        Querier.GetAllCityData()
                                                                                               .ToNgrams(ContextLength));
        _ = Directory.CreateDirectory(Path.Join(OUTPUT_DIRECTORY, GeneratorName));

        int numPerBiome   = CommandLineArgs.TryParseValue<int>(nameof(numPerBiome))   ?? 10,
            minCityLength = CommandLineArgs.TryParseValue<int>(nameof(minCityLength)) ??  5,
            maxCityLength = CommandLineArgs.TryParseValue<int>(nameof(maxCityLength)) ?? 40;

        
        NgramInfo query = new("Be", "", "Temperate Broadleaf & Mixed Forests");
        if (generator is MulticlassStringGenerator mc)
            TestMulticlassStringGenerator(mc, query);
        for (int i = 0; i < 10; i++)
            Console.WriteLine(generator.RandomString(query, 5, 20));
        MarkovSetStringGenerator control = MarkovSetStringGenerator.Load($"generators_{ContextLength}.json");
        for (int i = 0; i < 10; i++)
            Console.WriteLine(control.RandomString(query, 5, 20));
        return;
        foreach (string biome in DataProcessor.BiomeCache.Order())
        {
            Console.WriteLine(biome);
            string path = Path.Join(OUTPUT_DIRECTORY, GeneratorName, $"{biome.Replace("/", ",")}.txt");
            path.CreateIfNotExists();
            foreach (string name in generator.RandomStringsOfLength(NgramInfo.Query(biome), numPerBiome, minCityLength, maxCityLength))
                Utils.PrintAndWrite(path, name);
        }
    }
    private static void TestMulticlassStringGenerator(MulticlassStringGenerator mc, NgramInfo query)
    {
        File.WriteAllText("test.txt", "");
        using FileStream fs = File.OpenWrite("test.txt");
        using StreamWriter sw = new(fs);
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
}