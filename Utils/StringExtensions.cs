namespace citynames;
/// <summary>
/// Extensions which make working with strings easier.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Appends <paramref name="c"/> to the end of <paramref name="s"/> if <paramref name="s"/>
    /// does not already end with <paramref name="c"/>.
    /// </summary>
    /// <param name="s">The string to which to append <paramref name="c"/>.</param>
    /// <param name="c">The character to append to <paramref name="s"/>.</param>
    /// <returns>The string created as described above.</returns>
    public static string AppendIfNotPresent(this string s, char c) => s.EndsWith(c) ? s : $"{s}{c}";
    /// <summary>
    /// Gets the last <paramref name="n"/> characters of <paramref name="s"/>. If 
    /// <paramref name="s"/> has fewer than <paramref name="n"/> characters, the entire string is
    /// returned without modification.
    /// </summary>
    /// <param name="s">The string whose last <paramref name="n"/> digits to get.</param>
    /// <param name="n">The number of characters from the end of the string to return.</param>
    /// <returns>The string created as described above.</returns>
    public static string Last(this string s, int n)
        => SubstringSafe(s, s.Length - n, s.Length);
    /// <summary>
    /// Replaces any segments of <paramref name="s"/> containing a key in <paramref name="dict"/>
    /// surrounded by curly braces (<c>{}</c>), e.g. <c>{key}</c>, with the <c>ToString</c> of the
    /// corresponding value.
    /// </summary>
    /// <param name="s">The string whose keys to replace.</param>
    /// <param name="dict">A dictionary of keys and the values to replace them with.</param>
    /// <returns>The string created as described above.</returns>
    public static string ReplaceUsing(this string s, IReadOnlyDictionary<string, object?> dict)
    {
        string result = s;
        foreach ((string key, object? value) in dict)
            if (value is not null)
                result = s.Replace($"{{{key}}}", $"{value}");
        return result;
    }
    /// <summary>
    /// Gets the substring starting at <paramref name="start"/>, inclusive, and ending at 
    /// <paramref name="end"/>, exclusive, of <paramref name="s"/>. If either of these values
    /// exceeds the bounds of the string, returns a string which starts or ends at the appropriate
    /// value rather than throwing an error.
    /// </summary>
    /// <param name="s">The string to get a substring of.</param>
    /// <param name="start">The starting position, <b>inclusive</b>, of the resulting substring.</param>
    /// <param name="end">The ending position, <b>exclusive</b>, of the resulting substring.</param>
    /// <returns>The substring as described above.</returns>
    /// <remarks><b>Note:</b> this method uses a start and an end position, like string slicing,
    /// rather than the start and substring length that the normal <see cref="string.Substring(int, int)"/>
    /// uses.</remarks>
    public static string SubstringSafe(this string s, int start, int end)
    {
        start = Math.Max(start, 0);
        end = Math.Min(end, s.Length);
        return s[start..end];
    }
    /// <summary>
    /// Produces a <see href="https://en.wikipedia.org/wiki/One-hot#Natural_language_processing">
    /// one-hot encoding</see> of <paramref name="s"/>, with other keys optionally encoded as 0.
    /// </summary>
    /// <param name="s">The string whose weight will be encoded as 1.</param>
    /// <param name="otherKeys">The other keys, whose weights will be encoded as 0. If not specified,
    /// no other keys will be included.</param>
    /// <returns>A dictionary representing the above-described encoding.</returns>
    public static IReadOnlyDictionary<string, float> ToWeightVector(this string s, IEnumerable<string>? otherKeys = null)
    {
        Dictionary<string, float> result = new()
        {
            { s, 1 }
        };
        if (otherKeys is not null)
            foreach (string key in otherKeys)
                result[key] = 0;
        return result;
    }
}