namespace citynames;
internal class MarginalProbabilityStringGenerator : IBuildLoadableStringGenerator<CharPair, MarginalProbabilityStringGenerator>
{
}
public class CharPair
{
    public char Ancestor, Successor;
    // e.g. -1 = {ancestor}{successor}, -2 = {ancestor}.{successor}, -3 = {ancestor}..{successor}
    public int Offset;
    public QueryInfo Data;
}
public class QueryInfo
{
    public Dictionary<string, float> BiomeWeights;
}