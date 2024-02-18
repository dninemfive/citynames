using citynames;
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
        MulticlassStringGenerator generator = await MulticlassStringGenerator.LoadAsync("transformedData.csv");
        // PrintPreview(dataView, 250);
        // IDataView predictions = model.Transform(dataView);
        //MulticlassClassificationMetrics metrics = mlContext.MulticlassClassification.Evaluate(predictions);
        //Console.WriteLine(metrics.PrettyPrint());
        await generator.SaveAsync();
        NgramInfo test = new("zy", 'Q', "Montane Grasslands & Shrublands");
        CharacterPrediction prediction = generator.Predict(test);
        Console.WriteLine($"{prediction}");
        return;
        int contextLength = CommandLineArgs.TryParseValue<int>(nameof(contextLength)) ?? 2;
        string generatorFilename = $"generators_{contextLength}.json";
        bool buildGenerator = CommandLineArgs.GetFlag("rebuild") || !File.Exists(generatorFilename);

        MarkovSetStringGenerator generatorSet 
            = await MarkovSetStringGenerator.BuildOrLoadAsync(!buildGenerator, 
                                                              generatorFilename, 
                                                              () => Querier.GetAllCityDataAsync()
                                                                           .ToNgramsAsync(contextLength), 
                                                              contextLength);

        await Querier.SaveCache().WithMessage("Saving cache");
        if (buildGenerator)
            await generatorSet.SaveAsync(generatorFilename)
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
            foreach (string name in generatorSet.RandomStringsOfLength(NgramInfo.FromQuery(biome), numPerBiome, minCityLength, maxCityLength))
                Utils.PrintAndWrite(path, name);
        }
    }
}