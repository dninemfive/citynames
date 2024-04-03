using MathNet.Numerics;
using System.Text.Json.Serialization;

namespace citynames.String_Generation.RegressionStringGenerator;
public class BiomeCharacterRegression2
{
    [JsonInclude]
    public OneHotEncoding<string> BiomeEncoding { get; private set; }
    [JsonInclude]
    public OneHotEncoding<char> CharacterEncoding { get; private set; }
    [JsonInclude]
    public int MaxOffset { get; private set; }
    [JsonInclude]
    public Dictionary<int, Dictionary<char, double[]>> Coefficients { get; private set; }
    [JsonConstructor]
    public BiomeCharacterRegression2(OneHotEncoding<string> biomeEncoding,
                                     OneHotEncoding<char> characterEncoding,
                                     int maxOffset,
                                     Dictionary<int, Dictionary<char, double[]>> coefficients)
    {
        BiomeEncoding = biomeEncoding;
        CharacterEncoding = characterEncoding;
        MaxOffset = maxOffset;
        Coefficients = coefficients;
    }
    public static double[] Encode(CharPair pair, OneHotEncoding<string> biomeEncoding, OneHotEncoding<char> characterEncoding)
        => biomeEncoding.Encode(pair.Data.BiomeWeights).AugmentWith(characterEncoding.Encode(pair.Ancestor));
    public static BiomeCharacterRegression2 FromData(IEnumerable<(string city, CityInfo metadata)> data, int maxOffset)
    {
        Console.WriteLine(LogUtils.Method(args: [(nameof(data), data), (nameof(maxOffset), maxOffset)]));
        int tabCt = 1;
        OneHotEncoding<string> biomeEncoding = OneHotEncoding<string>.From(data.Select(x => x.metadata.Biome));
        OneHotEncoding<char> characterEncoding = OneHotEncoding<char>.From(data.SelectMany(x => x.city));
        List<CharPair> pairs = CharPair.From(data, maxOffset).ToList();
        List<double[]> encodedData = LogUtils.LogInvocation("Encoding data", () => pairs.Select(x => Encode(x, biomeEncoding, characterEncoding)).ToList());
        for(int i = 1; i <= maxOffset; i++)
        {
            Console.WriteLine($"  Offset: {i}");
            foreach(char character in characterEncoding.Alphabet)
            {
                double[] coefs = Fit.MultiDim()
            }
        }
    }
}
