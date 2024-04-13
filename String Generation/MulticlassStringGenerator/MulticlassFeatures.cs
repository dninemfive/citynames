﻿namespace citynames;
public class MulticlassFeatures(float[] biomeWeights, string[] ancestors, string result)
{

    public float[] BiomeWeights = biomeWeights;
    public string[] Ancestors = ancestors;
    public string Result = result;
    public MulticlassFeatures(IReadOnlyDictionary<string, float> biomeWeights, string[] ancestors, VectorEncoding<string, float> biomeEncoding, string result)
        : this(biomeEncoding.Encode(biomeWeights), ancestors, result) { }
    public MulticlassFeatures(string biome, string[] ancestors, VectorEncoding<string, float> biomeEncoding, string result)
        : this(biomeEncoding.Encode(biome), ancestors, result) { }
    public static IEnumerable<MulticlassFeatures> From(string cityName, CityInfo cityInfo, VectorEncoding<string, float> biomeEncoding, int contextLength = Defaults.CONTEXT_LENGTH)
    {
        Console.WriteLine(LogUtils.Method(args: [(nameof(cityName), cityName), (nameof(cityInfo), cityInfo), (nameof(biomeEncoding), biomeEncoding)]));
        for(int i = 0; i < cityName.Length; i++)
        {
            string[] ancestors = new string[contextLength];
            for(int j = 0; j < contextLength; j++)
            {
                int ancestorIndex = i - (contextLength - j);
                ancestors[j] = ancestorIndex >= 0 ? $"{cityName[ancestorIndex]}" : "";
            }
            yield return new(cityInfo.Biome, ancestors, biomeEncoding, $"{cityName[i]}");
        }
    }
    public static IEnumerable<MulticlassFeatures> From(IEnumerable<(string cityName, CityInfo cityInfo)> corpus,
                                                       VectorEncoding<string, float> biomeEncoding,
                                                       int contextLength = Defaults.CONTEXT_LENGTH)
        => corpus.SelectMany(x => From(x.cityName, x.cityInfo, biomeEncoding, contextLength));
    public static MulticlassFeatures Query(float[] biomeWeights)
        => Query(biomeWeights, []);
    public static MulticlassFeatures Query(float[] biomeWeights, string[] ancestors)
        => new(biomeWeights, ancestors, "");
}