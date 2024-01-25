using citynames;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Dictionary<string, MarkovStringGenerator> generatorsByBiome = new();
        await foreach((string city, string biome) in Querier.GetAllCities())
        {
            if(!generatorsByBiome.TryGetValue(biome, out MarkovStringGenerator? generator))
            {
                generator = new();
                generatorsByBiome[biome] = generator;
            }
            generator.Add(city);
        }
        Console.WriteLine("Data loaded.");
        Querier.SaveCache();
        Console.WriteLine("Generating city names...");
        _ = Directory.CreateDirectory("output");
        foreach (string biome in generatorsByBiome.Keys.Order())
        {
            string fileName = $"output/{biome.Replace("/", ",")}.txt";
            Console.WriteLine($"{biome}:");
            File.WriteAllText(fileName, "");
            for (int i = 0; i < 10; i++)
            {
                string cityName = generatorsByBiome[biome].RandomStringOfLength(min: 5, max: 40);
                Console.WriteLine($"\t{cityName}");
                File.AppendAllText(fileName, $"{cityName}\n");
            }
        }
    }
}