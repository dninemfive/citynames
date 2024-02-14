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

        MarkovSetStringGenerator generatorSet = await (buildGenerators ? BuildGenerator(contextLength) 
                                                           : LoadGenerator(generatorFilename))
                                          .WithMessage($"{(buildGenerators ? "Build" : "Load")}ing generators");

        await Querier.SaveCache().WithMessage("Saving cache");
        if (buildGenerators)
            await SaveGenerators(generatorFilename, generatorSet)
                  .WithMessage("Saving generators");
        _ = Directory.CreateDirectory(OUTPUT_DIRECTORY);

        int numPerBiome   = CommandLineArgs.TryParseValue<int>(nameof(numPerBiome))   ?? 10,
            minCityLength = CommandLineArgs.TryParseValue<int>(nameof(minCityLength)) ?? 5,
            maxCityLength = CommandLineArgs.TryParseValue<int>(nameof(maxCityLength)) ?? 40;

        void PrintAndWrite(string path, string s)
        {
            Console.WriteLine($"\t{s}");
            File.AppendAllText(path, $"{s}\n");
        }
        void CreateIfNotExists(string path)
        {
            if (!File.Exists(path))
                File.CreateText(path);
        }
        foreach (string biome in generatorSet.Biomes.Order())
        {
            Console.WriteLine(biome);
            string path = $"{biome.Replace("/", ",")}.txt";
            CreateIfNotExists(path);
            foreach (string name in GenerateNamesFor(biome, generatorSet, numPerBiome, minCityLength, maxCityLength))
                PrintAndWrite(path, name);
        }
    }
    private static async Task<MarkovSetStringGenerator> BuildGenerator(int contextLength)
    {
        Console.Write($"Building generators...");
        MarkovSetStringGenerator result = new();
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
    private static async Task<MarkovSetStringGenerator> LoadGenerator(string fileName)
        => new(await Task.Run(() => JsonSerializer.Deserialize<Dictionary<string, MarkovStringGenerator>>(File.ReadAllText(fileName))!));
    private static async Task SaveGenerators(string fileName, MarkovSetStringGenerator generator)
        => await Task.Run(() => File.WriteAllText(fileName, JsonSerializer.Serialize(generator)));
    private static IEnumerable<string> GenerateNamesFor<T>(T input, IStringGenerator<T> generator, int number, int minLength = 5, int maxLength = 40, int attemptsPerResult = 100)
        where T : notnull
    {
        for (int i = 0; i < number; i++)
            yield return generator.RandomStringOfLength(input, minLength, maxLength, attemptsPerResult);
    }
}