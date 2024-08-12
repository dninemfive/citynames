using d9.utl;
using MathNet.Numerics;
using MathNet.Numerics.LinearRegression;
using System.Text;

namespace citynames;
public static class Program
{
    public const string OUTPUT_DIRECTORY = "output";
    private static async Task Main()
    {
        Console.OutputEncoding = Encoding.Unicode;
        int contextLength = CommandLineArgs.TryParseStruct<int>(nameof(contextLength))           ??  2,
            numPerBiome   = CommandLineArgs.TryParseStruct<int>(nameof(numPerBiome))             ?? 10,
            minCityLength = CommandLineArgs.TryParseStruct<int>(nameof(minCityLength))           ??  5,
            maxCityLength = CommandLineArgs.TryParseStruct<int>(nameof(maxCityLength))           ?? 20;

        static IEnumerable<(string, CityInfo)> buildFn() => DataLoader.GetAllCityData().Select(x => (x.city, new CityInfo(x.biome)));

        await TestGenerators(buildFn, new("Tundra"), numPerBiome, minCityLength, maxCityLength);
    }
    private static async Task TestGenerators<T>(CityDataProvider<T> dataProvider, T query, int count, int minLength, int maxLength)
    {
        List<(string, T)> data = dataProvider().ToList();
        foreach(GeneratorInfo info in GeneratorInfo.All)
        {
            _ = Directory.CreateDirectory(Path.Join("generators", info.Name));
            Console.WriteLine($"{info.Name}:");
            ISaveableStringGenerator<T> generator = await info.Instantiate(() => data);
            for (int i = 0; i < count; i++)
                Console.WriteLine($"\t{generator.RandomString(query, minLength, maxLength)}");
        }
    }
}