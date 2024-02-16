﻿using citynames;
using d9.utl;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
namespace citynames;
public class Program
{
    public const string OUTPUT_DIRECTORY = "output";
    public class Feature
    {
        [LoadColumn(0)]
        public string Context;
        [LoadColumn(1)]
        public string Successor;
        [LoadColumn(2)]
        public string Biome;
        [LoadColumn(3)]
        public float Count;
    }
    public class Label
    {
        [ColumnName("Score")]
        public float PredictedCount;
    }
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
        // DataProcessor.WriteCsv();
        MLContext mlContext = new();
        IDataView dataView = mlContext.Data.LoadFromTextFile<Feature>("transformedData.csv", new() { HasHeader = true, Separators = new char[] { ',' } });
        PrintPreview(dataView, 250);
        return;
        var pipeline = mlContext.Transforms.CopyColumns("Label", "Count")
                                           .Append(mlContext.Transforms.Categorical.OneHotEncoding("BiomeEncoded", "Biome"))
                                           .Append(mlContext.Transforms.Categorical.OneHotEncoding("ContextEncoded", "Context"))
                                           .Append(mlContext.Transforms.Categorical.OneHotEncoding("SuccessorEncoded", "Successor"))
                                           .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: "Count"))
                                           .Append(mlContext.Transforms.Concatenate("Features", "BiomeEncoded", "ContextEncoded", "SuccessorEncoded", "Count"))
                                           .Append(mlContext.Regression.Trainers.Sdca());
        var model = pipeline.Fit(dataView);
        IDataView predictions = model.Transform(dataView);
        RegressionMetrics metrics = mlContext.Regression.Evaluate(predictions);
        Console.WriteLine(metrics.PrettyPrint());
        mlContext.Model.Save(model, dataView.Schema, "model.zip");
        return;
        int contextLength = CommandLineArgs.TryParseValue<int>(nameof(contextLength)) ?? 2;
        string generatorFilename = $"generators_{contextLength}.json";
        bool buildGenerator = CommandLineArgs.GetFlag("rebuild") || !File.Exists(generatorFilename);

        MarkovSetStringGenerator generatorSet = await (buildGenerator ? BuildGenerator(contextLength) 
                                                           : LoadGenerator(generatorFilename))
                                          .WithMessage($"{(buildGenerator ? "Build" : "Load")}ing generator");

        await Querier.SaveCache().WithMessage("Saving cache");
        if (buildGenerator)
            await SaveGenerator(generatorFilename, generatorSet)
                  .WithMessage("Saving generators");
        _ = Directory.CreateDirectory(OUTPUT_DIRECTORY);

        int numPerBiome   = CommandLineArgs.TryParseValue<int>(nameof(numPerBiome))   ?? 10,
            minCityLength = CommandLineArgs.TryParseValue<int>(nameof(minCityLength)) ?? 5,
            maxCityLength = CommandLineArgs.TryParseValue<int>(nameof(maxCityLength)) ?? 40;

        foreach (string biome in generatorSet.Biomes.Order())
        {
            Console.WriteLine(biome);
            string path = $"{biome.Replace("/", ",")}.txt";
            path.CreateIfNotExists();
            foreach (string name in generatorSet.RandomStringsOfLength(biome, numPerBiome, minCityLength, maxCityLength))
                Utils.PrintAndWrite(path, name);
        }
    }
    private static async Task<MarkovSetStringGenerator> BuildGenerator(int contextLength)
    {
        Console.Write($"Building generators...");
        MarkovSetStringGenerator result = new();
        await foreach ((string city, string biome) in Querier.GetAllCityDataAsync())
        {
            if (!result.TryGetValue(biome, out MarkovStringGenerator? generator))
            {
                generator = new(contextLength);
                result[biome] = generator;
            }
            generator.Add(city.Split("(")[0].Split(",")[0]);
        }
        return result;
    }
    private static async Task<MarkovSetStringGenerator> LoadGenerator(string fileName)
        => new(await Task.Run(() => JsonSerializer.Deserialize<Dictionary<string, MarkovStringGenerator>>(File.ReadAllText(fileName))!));
    private static async Task SaveGenerator(string fileName, MarkovSetStringGenerator generator)
        => await Task.Run(() => File.WriteAllText(fileName, JsonSerializer.Serialize(generator)));
}