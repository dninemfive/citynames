
namespace citynames;
public class MarginalProbabilityStringGenerator
{
    public static MarginalProbabilityStringGenerator Build(IEnumerable<CharPair> input, int contextLength)
    {
        throw new NotImplementedException();
    }
    public static MarginalProbabilityStringGenerator Load(string path)
    {
        throw new NotImplementedException();
    }
    public char RandomChar(QueryInfo input, string context)
    {
        // map characters to weights:
        //  prior probability:
        //      for each (ancestor, offset) in ancestors,
        //          probability = (1/offset)(P(character|ancestor)
        //              **: could simply do an EWMA
        //  add marginal probabilities:
        //      for each (biome, weight) in input.biomeWeights,
        //          probability += (weight)(P(character|ancestors, biome))
        throw new NotImplementedException();
    }
    public string RandomString(QueryInfo input, int minLength, int maxLength)
    {
        string result = "";
        char cur = RandomChar(input, result);
        while(result.Length < maxLength && cur != Characters.STOP)
        {
            result += cur;
            cur = RandomChar(input, result);
        }
        return result;
    }
    public Task SaveAsync(string path)
    {
        throw new NotImplementedException();
    }
}
public class CharPair(char ancestor, char successor, int offset, QueryInfo data)
{
    public readonly char Ancestor = ancestor, Successor = successor;
    // e.g. 1 = {ancestor}{successor}, 2 = {ancestor}.{successor}, 3 = {ancestor}..{successor}
    public readonly int Offset = offset;
    public readonly QueryInfo Data = data;
    public static IEnumerable<CharPair> From(string cityName, string biome, int maxOffset = 4)
    {
        for(int i = 0; i < cityName.Length; i++)
        {
            char ancestor = cityName[i];
            for(int j = i + 1; i < cityName.Length; i++)
            {
                char successor = cityName[j];
                int offset = j - i;
                if (offset > maxOffset)
                    break;
                yield return new(ancestor, successor, offset, new(biome));
            }
        }
    }
    public static IEnumerable<CharPair> From((string cityName, string biome) tuple, int maxOffset = 4)
        => From(tuple.cityName, tuple.biome, maxOffset);
    public static IEnumerable<CharPair> From(IEnumerable<(string cityName, string biome)> tuples, int maxOffset = 4)
        => tuples.SelectMany(x => From(x, maxOffset)).ToList();
}
public class QueryInfo(Dictionary<string, float> biomeWeights)
{
    public IReadOnlyDictionary<string, float> BiomeWeights = biomeWeights;
    public QueryInfo(string biome) : this(biome.ToWeightVector()) { }
}