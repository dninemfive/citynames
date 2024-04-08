using d9.utl;
using MathNet.Numerics;
using MathNet.Numerics.LinearRegression;
using System.Text.Json.Serialization;

namespace citynames;
[method: JsonConstructor]
public class AncestorCharacterRegression(OneHotEncoding<string> biomeEncoding,
                                 OneHotEncoding<char> characterEncoding,
                                 Dictionary<int, Dictionary<char, double[]>> coefficients)
{
    [JsonInclude]
    public OneHotEncoding<string> BiomeEncoding { get; private set; } = biomeEncoding;
    [JsonInclude]
    public OneHotEncoding<char> CharacterEncoding { get; private set; } = characterEncoding;
    [JsonInclude]
    public Dictionary<int, Dictionary<char, double[]>> Coefficients { get; private set; } = coefficients;

    public double[] Encode(QueryInfo query, char ancestor)
        => Encode(query.BiomeWeights, ancestor, BiomeEncoding, CharacterEncoding);
    public static double[] Encode(IReadOnlyDictionary<string, double> biomeWeights,
                                  char ancestor,
                                  OneHotEncoding<string> biomeEncoding,
                                  OneHotEncoding<char> characterEncoding)
        //=> characterEncoding.Encode(ancestor).AugmentWith(biomeEncoding.Encode(biomeWeights));
        => biomeEncoding.Encode(biomeWeights).AugmentWith(characterEncoding.Encode(ancestor));
    public static double[] Encode(CharPair pair, OneHotEncoding<string> biomeEncoding, OneHotEncoding<char> characterEncoding)
        => Encode(pair.Data.BiomeWeights, pair.Ancestor, biomeEncoding, characterEncoding);
    public static AncestorCharacterRegression FromData(IEnumerable<(string city, CityInfo metadata)> data, int maxOffset)
    {
        Console.WriteLine(LogUtils.Method(args: [(nameof(data), data), (nameof(maxOffset), maxOffset)]));
        OneHotEncoding<string> biomeEncoding = OneHotEncoding<string>.From(data.Select(x => x.metadata.Biome));
        OneHotEncoding<char> characterEncoding = OneHotEncoding<char>.From(data.SelectMany(x => x.city));
        List<CharPair> pairs = CharPair.From(data, maxOffset).ToList();
        double[][] encodedData = LogUtils.LogAndTime($"{1.Tabs()}Encoding data",
                                                         () => pairs.Select(x => Encode(x, biomeEncoding, characterEncoding)).ToArray());
        Dictionary<int, Dictionary<char, double[]>> coefficientDict = new();
        for(int i = 1; i <= maxOffset; i++)
        {
            Console.WriteLine($"{2.Tabs()}Offset: {i}");
            coefficientDict[i] = new();
            foreach(char c in characterEncoding.Alphabet)
            {
                double[] coefs = LogUtils.LogAndTime($"{2.Tabs()} Fitting {c}/{(int)c}", 
                                                     () => Fit.MultiDim(encodedData,
                                                                        pairs.Select(x => x.Result == c ? 1.0 : 0.0).ToArray(),
                                                                        intercept: false, 
                                                                        method: DirectRegressionMethod.QR));
                coefficientDict[i][c] = coefs;
            }
        }
        return new(biomeEncoding, characterEncoding, coefficientDict);
    }
    public double WeightFor(char ancestor, int offset, QueryInfo query)
    {
        double[] coefficients = Coefficients[offset][ancestor];
        double[] inputs = Encode(query, ancestor);
        double result = coefficients[0];
        for(int i = 0; i < inputs.Length; i++)
            result += coefficients[i + 1] * inputs[i];
        return result;
    }
    public IReadOnlyDictionary<char, double> WeightsFor(QueryInfo query, string context)
    {
        Dictionary<char, double> result = new();
        foreach(int offset in Coefficients.Keys)
        {
            if (offset >= context.Length)
                continue;
            char ancestor = context[^offset];
            foreach (char c in Coefficients[offset].Keys)
                result[c] = WeightFor(c, offset, query) / offset;
        }
        return result;
    }
}
