namespace citynames;
public class QueryInfo(IReadOnlyDictionary<string, double> biomeWeights)
{
    public IReadOnlyDictionary<string, double> BiomeWeights = biomeWeights;
    public QueryInfo(string biome) : this(biome.ToWeightVector()) { }
}