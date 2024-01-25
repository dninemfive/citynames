using citynames;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Dictionary<string, MarkovStringGenerator> generatorsByBiome = new();
        await foreach((string city, string biome) in Querier.GetAllCities())
        {
            Console.WriteLine($"{city,-64}\t{biome}");
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
        Querier.SaveCache();
    }
}