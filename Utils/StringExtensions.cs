namespace citynames;
public static class StringExtensions
{
    public static string AppendIfNotPresent(this string s, char c) => s.EndsWith(c) ? s : $"{s}{c}";
    public static string Last(this string str, int n)
        => SubstringSafe(str, str.Length - n, str.Length);
    public static string ReplaceUsing(this string s, IReadOnlyDictionary<string, object?> dict)
    {
        string result = s;
        foreach ((string key, object? value) in dict)
            if (value is not null)
                result = s.Replace($"{{key}}", $"{value}");
        return result;
    }
    public static string SubstringSafe(this string str, int start, int end)
    {
        start = Math.Max(start, 0);
        end = Math.Min(end, str.Length);
        return str[start..end];
    }
    public static Dictionary<string, float> ToWeightVector(this string str) => new()
    {
        { str, 1 }
    };
}