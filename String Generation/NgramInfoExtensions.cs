namespace citynames;
public static class NgramInfoExtensions
{
    public static IEnumerable<NgramInfo> NgramInfos(this string cityName, string biome, int contextLength = 2, string breakChars = ",(")
    {
        cityName = cityName.AppendIfNotPresent(Characters.STOP);
        string cur = "";
        for (int i = 1 - contextLength; i <= cityName.Length - contextLength; i++)
        {
            string successor = $"{cityName[i + contextLength - 1]}";
            if (breakChars.Contains(successor))
                break;
            yield return new(cur, successor, biome);
            cur = cityName.SubstringSafe(i, i + contextLength);
        }
    }
    public static IEnumerable<NgramInfo> ToNgrams(this IEnumerable<(string cityName, string biome)> rawData, int contextLength = 2, string breakChars = ",(")
    {
        foreach ((string cityName, string biome) in rawData)
            foreach (NgramInfo ngram in cityName.NgramInfos(biome, contextLength, breakChars))
                yield return ngram;
    }
}
