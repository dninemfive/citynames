using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames;
public class MulticlassFeatures(float[] biomeWeights, string[] ancestors, VectorEncoding<string, float> biomeEncoding, string result)
{

    public float[] BiomeWeights = biomeWeights;
    public string[] Ancestors = ancestors;
    public string Result = result;
    public VectorEncoding<string, float> BiomeEncoding { get; private set; } = biomeEncoding;
    public MulticlassFeatures(IReadOnlyDictionary<string, float> biomeWeights, string[] ancestors, VectorEncoding<string, float> biomeEncoding, string result)
        : this(biomeEncoding.Encode(biomeWeights), ancestors, biomeEncoding, result) { }
    public MulticlassFeatures(string biome, string[] ancestors, VectorEncoding<string, float> biomeEncoding, string result)
        : this(biomeEncoding.Encode(biome), ancestors, biomeEncoding, result) { }
    public static IEnumerable<MulticlassFeatures> From(string cityName, CityInfo cityInfo, VectorEncoding<string, float> biomeEncoding, int contextLength = 3)
    {
        for(int i = 0; i < cityName.Length; i++)
        {
            string[] ancestors = new string[contextLength];
            for(int j = i - contextLength; j < i; i++)
                ancestors[j] = j >= 0 ? $"{cityName[j]}" : "";
            yield return new(cityInfo.Biome, ancestors, biomeEncoding, $"{cityName[i]}");
        }
    }
    public static IEnumerable<MulticlassFeatures> From(IEnumerable<(string cityName, CityInfo cityInfo)> corpus,
                                                       VectorEncoding<string, float> biomeEncoding,
                                                       int contextLength = 3)
        => corpus.SelectMany(x => From(x.cityName, x.cityInfo, biomeEncoding, contextLength));
}