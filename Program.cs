using d9.utl;
using MathNet.Numerics;
using MathNet.Numerics.LinearRegression;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using System.Text;

namespace citynames;
public class ExampleTestThing(Dictionary<string, float> stuff)
{
    public Dictionary<string, float> Stuff = stuff;
}
public static class Program
{
    public const string OUTPUT_DIRECTORY = "output";
    private static async Task Main()
    {
        MLContext mlContext = new();
        ExampleTestThing exampleTestThing = new(new
        Dictionary<string, float>()
        {
            { "fuck", 0.9f },
            { "cum", 4.20f },
            { "shit", 5 }
        });
        IDataView data = mlContext.Data.LoadFromEnumerable([exampleTestThing]);
        OneHotEncodingEstimator pipeline = mlContext.Transforms.Categorical.OneHotEncoding("StuffEncoded", "Stuff");
        IDataView tfedData = pipeline.Fit(data).Transform(data);
        static void printDataColumn(IDataView tfedData, string columnName)
        {
            IEnumerable<float[]> asdf = tfedData.GetColumn<float[]>(tfedData.Schema[columnName]);
            foreach (float[] row in asdf)
            {
                Console.WriteLine(row.ListNotation(brackets: null));
            }
        }
        printDataColumn(tfedData, "StuffEncoded");
        return;
        Console.OutputEncoding = Encoding.Unicode;
        int contextLength = CommandLineArgs.TryParseValue<int>(nameof(contextLength))           ?? 2,
            numPerBiome   = CommandLineArgs.TryParseValue<int>(nameof(numPerBiome))             ?? 10,
            minCityLength = CommandLineArgs.TryParseValue<int>(nameof(minCityLength))           ??  5,
            maxCityLength = CommandLineArgs.TryParseValue<int>(nameof(maxCityLength))           ?? 20;

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