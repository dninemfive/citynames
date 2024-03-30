namespace citynames;
/// <summary>
/// A couple extensions to make generating <see cref="NgramInfo"/>s more convenient.
/// </summary>
public static class NgramInfoExtensions
{
    /// <summary>
    /// Converts a specific <paramref name="cityName"/>-<paramref name="biome"/> pair into an
    /// <c>NgramInfo</c>s with the specified <paramref name="contextLength"/> until it reaches any
    /// of the <paramref name="breakChars"/> or the end of the city name.
    /// </summary>
    /// <param name="cityName">The city name to break into <c>NgramInfo</c>s.</param>
    /// <param name="biome">The biome associated with the city and its <c>NgramInfo</c>s.</param>
    /// <param name="contextLength">The maximum number of characters preserved by each
    ///                             <c>NgramInfo</c>.</param>
    /// <param name="breakChars">A string of characters which will cause the method to stop yielding
    ///                         <c>NgramInfo</c>s and return early.</param>
    /// <returns>A collection of <c>NgramInfo</c>s as described above.</returns>
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
    /// <summary>
    /// Converts a collection of <c>cityName</c>-<c>biome</c> pairs into <c>NgramInfo</c>s with the
    /// specified <paramref name="contextLength"/>, breaking at the specified <paramref name="breakChars"/>
    /// as described at <see cref="NgramInfos(string, string, int, string)"/>.
    /// </summary>
    /// <param name="rawData">The collection of <c>cityName</c>-<c>biome</c> pairs whose
    ///                       <c>NgramInfos</c> to generate.</param>
    /// <param name="contextLength">
    /// <inheritdoc cref="NgramInfos(string, string, int, string)"
    ///             path="/param[@name='contextLength']/node()"/>
    /// </param>
    /// <param name="breakChars">A string of characters at which each city name will stop yielding
    ///                         <c>NgramInfo</c>s and the method will continue to the next item.</param>
    /// <returns>A collection of <c>NgramInfo</c>s describing the ngrams found in the
    ///          <paramref name="rawData"/>.</returns>
    public static IEnumerable<NgramInfo> ToNgrams(this IEnumerable<(string cityName, string biome)> rawData, int contextLength = 2, string breakChars = ",(")
    {
        foreach ((string cityName, string biome) in rawData)
            foreach (NgramInfo ngram in cityName.NgramInfos(biome, contextLength, breakChars))
                yield return ngram;
    }
}
