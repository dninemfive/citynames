﻿using d9.utl;

namespace citynames;
public class Program
{
    public const string OUTPUT_DIRECTORY = "output";
    private static async Task Main()
    {
        List<(string cityName, CityInfo info)> corpus = [
            ("Aarhus", new("fuck")),
            ("Rio de Janeiro", new("fuck")),
            ("6th of June 1944 City", new("fuck")),
            ("Alexandria", new("fuck")),
            ("Berlin", new("fuck")),
            ("Ahmedabad", new("fuck")),
            ("Jordan", new("fuck")),
            ("Paris", new("fuck")),
            ("Phoenix", new("fuck")),
            ("Persepolis", new("fuck"))
        ];
        RegressionStringGenerator rsg = RegressionStringGenerator.Build(corpus, 2);
        await rsg.SaveAsync("regression test.json");
        // Console.WriteLine(rsg.RandomString(new("fuck"), 5, 10));
        return;
        int contextLength        = CommandLineArgs.TryParseValue<int>(nameof(contextLength)) ?? 2;
        string testGeneratorName = CommandLineArgs.TryGet("generator", CommandLineArgs.Parsers.FirstNonNullOrEmptyString) ?? "markov";

        GeneratorInfo testGeneratorInfo    = GeneratorInfo.GetByName(testGeneratorName),
                      controlGeneratorInfo = GeneratorInfo.GetByName("markov");
        IEnumerable<(string, CityInfo)> buildFn() => DataLoader.GetAllCityData().Select(x => (x.city, new CityInfo(x.biome)));
        ISaveableStringGenerator<CityInfo> test    = await testGeneratorInfo.Instantiate(contextLength, buildFn, CommandLineArgs.GetFlag("rebuild")),
                                           control = await controlGeneratorInfo.Instantiate(contextLength, buildFn, false);

        _ = Directory.CreateDirectory(Path.Join(OUTPUT_DIRECTORY, testGeneratorName));

        int numPerBiome   = CommandLineArgs.TryParseValue<int>(nameof(numPerBiome))   ?? 10,
            minCityLength = CommandLineArgs.TryParseValue<int>(nameof(minCityLength)) ??  5,
            maxCityLength = CommandLineArgs.TryParseValue<int>(nameof(maxCityLength)) ?? 20;

        CityInfo query = new("Tundra");
        if (test is MulticlassStringGenerator mc)
            TestMulticlassStringGenerator(mc, query);
        if (test is EnhancedMarkovStringGenerator emsg)
            for (int i = 0; i < 10; i++)
                Console.WriteLine(emsg.RandomString(new Dictionary<string, double>() { { "Temperate Conifer Forests", 0.1f }, { "Tundra", 0.9f } }, 0, 20));
        void writeCities(ISaveableStringGenerator<CityInfo> generator, string name)
        {
            Console.WriteLine($"{name}:");
            for (int i = 0; i < numPerBiome; i++)
                Console.WriteLine($"\t{generator.RandomString(query, minCityLength, maxCityLength)}");
        }
        writeCities(control, nameof(control));
        writeCities(test, nameof(test));
    }
    private static void TestMulticlassStringGenerator(MulticlassStringGenerator mc, CityInfo query)
    {
        File.WriteAllText("test.txt", "");
        using FileStream fs = File.OpenWrite("test.txt");
        using StreamWriter sw = new(fs);
        // Console.WriteLine(mc.KeyValueMapper.OrderBy(x => x.Key).Select(x => $"\n{x.Key}\t{x.Value}").ListNotation());
        // Console.WriteLine(mc.KeyValueMapper.Values.Count(x => mc.KeyValueMapper.Values.Count(y => x == y) > 1));
        CharacterPrediction prediction = mc.Predict(NgramInfo.Query(query.Biome));
        Console.WriteLine($"Total weight: {prediction.CharacterWeights.Sum()}");
        Console.WriteLine($"Weight Count: {prediction.CharacterWeights.Length}");
        Console.WriteLine($"Id Count:     {mc.KeyValueMapper.Keys.Count()}");
        float max = prediction.CharacterWeights.Max();
        foreach ((int id, string character) in mc.KeyValueMapper.OrderBy(x => x.Key))
        {
            sw.Write($"{id}\t");
            if (id < prediction.CharacterWeights.Length)
                sw.Write($"{prediction.CharacterWeights[id] / max:P2}");
            sw.WriteLine($"\t{character}");
        }
    }
}