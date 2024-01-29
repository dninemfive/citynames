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
        int contextLength = CommandLineArgs.TryParseValue<int>(nameof(contextLength)) ?? 2;
        string generatorFilename = $"generators_{contextLength}.json";
        bool buildGenerators = CommandLineArgs.GetFlag("rebuild") || !File.Exists(generatorFilename);

        GeneratorSet generatorSet = await (buildGenerators ? BuildGenerators(contextLength) 
                                                           : LoadGenerators(generatorFilename))
                                          .WithMessage($"{(buildGenerators ? "Build" : "Load")}ing generators");

        await Querier.SaveCache().WithMessage("Saving cache");
        if (buildGenerators)
            await SaveGenerators(generatorFilename, generatorSet)
                  .WithMessage("Saving generators");
        _ = Directory.CreateDirectory(OUTPUT_DIRECTORY);

        int numPerBiome   = CommandLineArgs.TryParseValue<int>(nameof(numPerBiome))   ?? 10,
            minCityLength = CommandLineArgs.TryParseValue<int>(nameof(minCityLength)) ?? 5,
            maxCityLength = CommandLineArgs.TryParseValue<int>(nameof(maxCityLength)) ?? 40;
        foreach (string biome in generatorSet.Biomes.Order())
            GenerateNamesFor(biome, generatorSet, numPerBiome, minCityLength, maxCityLength);
    }
    private static async Task<GeneratorSet> BuildGenerators(int contextLength)
    {
        Console.Write($"Building generators...");
        GeneratorSet result = new();
        await foreach ((string city, string biome) in Querier.GetAllCities())
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
    private static async Task<GeneratorSet> LoadGenerators(string fileName)
        => new(await Task.Run(() => JsonSerializer.Deserialize<Dictionary<string, MarkovStringGenerator>>(File.ReadAllText(fileName))!));
    private static async Task SaveGenerators(string fileName, GeneratorSet generatorsByBiome)
        => await Task.Run(() => File.WriteAllText(fileName, JsonSerializer.Serialize(generatorsByBiome)));
    private static void GenerateNamesFor(string biome, GeneratorSet generators, int number, int minLength = 5, int maxLength = 40)
    {
        string fileName = $"{biome.Replace("/", ",")}.txt";
        Console.WriteLine($"{biome}:");
        string filePath = Path.Join(OUTPUT_DIRECTORY, fileName);
        if (!File.Exists(filePath))
            File.WriteAllText(filePath, "");
        for (int i = 0; i < number; i++)
        {
            string cityName = generators[biome].RandomStringOfLength(minLength, maxLength);
            Console.WriteLine($"\t{cityName}");
            File.AppendAllText(filePath, $"{cityName}\n");
        }
    }
}