namespace citynames;
public class BiomeCharacterRegressionSet
{
    public OneHotEncoding<string> BiomeEncoding { get; private set; }
    public OneHotEncoding<char> AncestorEncoding { get; private set; }
    public int MaxOffset { get; private set; }
    private readonly Dictionary<int, BiomeCharacterRegression> _regressions = new();
    public BiomeCharacterRegressionSet(OneHotEncoding<string> biomeEncoding, OneHotEncoding<char> ancestorEncoding, int maxOffset)
    {
        BiomeEncoding = biomeEncoding;
        AncestorEncoding = ancestorEncoding;
        MaxOffset = maxOffset;
        for (int i = 1; i < maxOffset; i++)
            _regressions[i] = new(biomeEncoding, ancestorEncoding, i);
    }
    public IReadOnlyDictionary<char, double> WeightsFor(string biome, char ancestor)
    {
        DefaultDict<char, double> result = new(default(double));
        foreach ((int offset, BiomeCharacterRegression regression) in _regressions)
        {
            foreach ((char character, double weight) in regression.WeightsFor(biome, ancestor))
                result[character] += weight;
        }
        return (IReadOnlyDictionary<char, double>)result;
    }
}