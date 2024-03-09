namespace citynames;
public static class StringUtils
{
    public static string SubstringSafe(this string str, int start, int end)
    {
        start = Math.Max(start, 0);
        end = Math.Min(end, str.Length);
        return str[start..end];
    }
    public static string Last(this string str, int n)
        => SubstringSafe(str, str.Length - n, str.Length);
    public static bool EndsWith(this string s, char c) => s.Length > 0 && s[^1] == c;
    public static string AppendIfNotPresent(this string s, char c) => s.EndsWith(c) ? s : $"{s}{c}";
    public static void CreateIfNotExists(this string path, string initialText = "")
    {
        if (!File.Exists(path))
            File.WriteAllText(path, initialText);
    }
    private static readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();
    public static bool IsInvalidFileNameChar(this char c) => _invalidFileNameChars.Contains(c);
    public static string FileNameSafe(this string s)
    {
        string result = string.Empty;
        foreach (char c in s)
            result += c.IsInvalidFileNameChar() ? $"`{(int)c}`" : c;
        return result;
    }
    public static string NaturalLanguageList(this IEnumerable<object> objects, string conjunction = "or")
        => objects.Count() switch
        {
            0 => "",
            1 => $"{objects.First()}",
            2 => $"{objects.First()} {conjunction} {objects.Last()}",
            _ => $"{objects.SkipLast(1).Aggregate((x, y) => $"{x}, {y}")}, {conjunction} {objects.Last()}"
        };
    public static string ReplaceUsing(this string s, IReadOnlyDictionary<string, object?> dict)
    {
        string result = s;
        foreach ((string key, object? value) in dict)
            if (value is not null)
                result = s.Replace($"{{key}}", $"{value}");
        return result;
    }
}