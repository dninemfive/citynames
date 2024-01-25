using citynames;
using d9.utl;
using System.Text.Json;

public class Program
{
    public const string GENERATOR_FILENAME = "generators.json";
    public const string OUTPUT_DIRECTORY = "output";
    private static async Task Main(string[] args)
    {
        bool regenerate = CommandLineArgs.GetFlag("regenerate");
        Dictionary<string, MarkovStringGenerator> generatorsByBiome;
        if(regenerate || !File.Exists(GENERATOR_FILENAME))
        {
            generatorsByBiome = new();
            await foreach ((string city, string biome) in Querier.GetAllCities())
            {
                if (!generatorsByBiome.TryGetValue(biome, out MarkovStringGenerator? generator))
                {
                    generator = new();
                    generatorsByBiome[biome] = generator;
                }
                generator.Add(city.Split("(")[0].Split(",")[0]);
            }
        } 
        else
        {
            generatorsByBiome = JsonSerializer.Deserialize<Dictionary<string, MarkovStringGenerator>>(File.ReadAllText(GENERATOR_FILENAME))!;
        }
        Console.WriteLine("Data loaded.");
        Querier.SaveCache();
        if(regenerate)
        {
            Console.Write($"Saving generators...");
            File.WriteAllText(GENERATOR_FILENAME, JsonSerializer.Serialize(generatorsByBiome));
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
            for (int i = 0; i < 10; i++)
            {
                string cityName = generatorsByBiome[biome].RandomStringOfLength(min: 5, max: 40);
                Console.WriteLine($"\t{cityName}");
                File.AppendAllText(fileName, $"{cityName}\n");
            }
        }
    }
}