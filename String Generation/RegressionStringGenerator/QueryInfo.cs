namespace citynames;
public class QueryInfo(IReadOnlyDictionary<string, float> biomeWeights)
{
    public IReadOnlyDictionary<string, float> BiomeWeights = biomeWeights;
    public QueryInfo(string biome) : this(biome.ToWeightVector()) { }
}