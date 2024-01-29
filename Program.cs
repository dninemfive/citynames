using citynames;
using d9.utl;
using System.Text.Json;
namespace citynames;
public class Program
{
    public static string GeneratorFilename { get; private set; } = "";
    public const string OUTPUT_DIRECTORY = "output";
    private static async Task Main()
    {
        int numPerBiome = CommandLineArgs.TryParseValue<int>("numPerBiome") ?? 10,
            contextLength = CommandLineArgs.TryParseValue<int>("contextLength") ?? 2;
        GeneratorFilename = $"generators_{contextLength}.json";
        bool generate = CommandLineArgs.GetFlag("regenerate") || !File.Exists(GeneratorFilename);
        Dictionary<string, MarkovStringGenerator> generatorsByBiome;
        if(generate)
        {
            generatorsByBiome = new();
            await foreach ((string city, string biome) in Querier.GetAllCities())
            {
                if (!generatorsByBiome.TryGetValue(biome, out MarkovStringGenerator? generator))
                {
                    generator = new(contextLength);
                    generatorsByBiome[biome] = generator;
                }
                generator.Add(city.Split("(")[0].Split(",")[0]);
            }
        } 
        else
        {
            generatorsByBiome = JsonSerializer.Deserialize<Dictionary<string, MarkovStringGenerator>>(File.ReadAllText(GeneratorFilename))!;
        }
        Console.WriteLine("Data loaded.");
        Querier.SaveCache();
        if(generate)
        {
            Console.Write($"Saving generators...");
            File.WriteAllText(GeneratorFilename, JsonSerializer.Serialize(generatorsByBiome));
            Console.WriteLine($"...Done!");
        }
        Console.WriteLine("Generating city names...");
        _ = Directory.CreateDirectory(OUTPUT_DIRECTORY);
        foreach (string biome in generatorsByBiome.Keys.Order())
        {
            string fileName = $"{biome.Replace("/", ",")}.txt";
            Console.WriteLine($"{biome}:");
            string filePath = Path.Join(OUTPUT_DIRECTORY, fileName);
            if(!File.Exists(filePath)) File.WriteAllText(filePath, "");
            for (int i = 0; i < numPerBiome; i++)
            {
                string cityName = generatorsByBiome[biome].RandomStringOfLength(min: 5, max: 40);
                Console.WriteLine($"\t{cityName}");
                File.AppendAllText(filePath, $"{cityName}\n");
            }
        }
    }
}