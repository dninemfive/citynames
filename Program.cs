using citynames;
using d9.utl;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
namespace citynames;
public class Program
{
    public const string OUTPUT_DIRECTORY = "output";
    private static async Task Main()
    {
        File.WriteAllLines("data.csv", DataProcessor.CsvLines(Querier.GetAllCityDataAsync().ToBlockingEnumerable()));
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