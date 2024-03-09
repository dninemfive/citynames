namespace citynames;
public static class NgramInfoExtensions
{
    public static IEnumerable<NgramInfo> NgramInfos(this string cityName, string biome, int contextLength = 2)
    {
        cityName = cityName.AppendIfNotPresent(Characters.STOP);
        string cur = "";
        for (int i = 1 - contextLength; i <= cityName.Length - contextLength; i++)
        {
            yield return new(cur, $"{cityName[i + contextLength - 1]}", biome);
            cur = cityName.SubstringSafe(i, i + contextLength);
        }
    }
    private static IEnumerable<NgramInfo> ToNgramsInternal(this string cityName, string biome, int contextLength = 2, string breakChars = ",(")
    {
        foreach (NgramInfo ngram in cityName.NgramInfos(biome, contextLength))
        {
            if (breakChars.Contains(ngram.Successor))
                break;
            yield return ngram;
        }
    }
    public static IEnumerable<NgramInfo> ToNgrams(this IEnumerable<(string cityName, string biome)> rawData, int contextLength = 2, string breakChars = ",(")
    {
        foreach ((string cityName, string biome) in rawData)
            foreach (NgramInfo ngram in cityName.ToNgramsInternal(biome, contextLength, breakChars))
                yield return ngram;
    }
    public static async IAsyncEnumerable<NgramInfo> ToNgramsAsync(this IAsyncEnumerable<(string cityName, string biome)> rawData, int contextLength = 2, string breakChars = ",(")
    {
        await foreach ((string cityName, string biome) in rawData)
            foreach (NgramInfo ngram in cityName.ToNgramsInternal(biome, contextLength, breakChars))
                yield return ngram;
    }
}
