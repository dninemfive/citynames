using d9.utl;
using MathNet.Numerics;
using MathNet.Numerics.LinearRegression;
using System.Text;

namespace citynames;
public class Program
{
    public const string OUTPUT_DIRECTORY = "output";
    private static async Task Main()
    {
        Console.OutputEncoding = Encoding.Unicode;
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
    private static void WriteCities(ISaveableStringGenerator<CityInfo> generator, string name)
    {

    }
}