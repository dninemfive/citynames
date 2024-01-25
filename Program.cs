using citynames;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine(await Querier.GetBiomeAsync(-118.74, 34.532395));
        return;
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
        foreach(string biome in generatorsByBiome.Keys.Order())
        {
            Console.WriteLine($"{biome}:");
            for (int i = 0; i < 10; i++)
                Console.WriteLine($"\t{generatorsByBiome[biome].RandomStringOfLength(5)}");
        }
    }
}