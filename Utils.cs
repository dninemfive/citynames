using Microsoft.ML.Data;
using Microsoft.ML;

namespace citynames;
public static class Utils
{
    public static string SubstringSafe(this string str, int start, int end)
    {
        start = Math.Max(start, 0);
        end = Math.Min(end, str.Length);
        return str[start..end];
    }
    public static string Last(this string str, int n)
        => SubstringSafe(str, str.Length - n, str.Length);
    public static async Task WithMessage(this Task task, string message)
    {
        Console.WriteLine($"{message}...");
        await task;
        Console.WriteLine("Done!");
    }
    public static async Task<T> WithMessage<T>(this Task<T> task, string message)
    {
        Console.WriteLine($"{message}...");
        T? result = await task;
        Console.WriteLine("Done!");
        return result;
    }
    public static bool EndsWith(this string s, char c) => s.Length > 0 && s[^1] == c;
    public static string AppendIfNotPresent(this string s, char c) => s.EndsWith(c) ? s : $"{s}{c}";
    public static void PrintAndWrite(string path, string s)
    {
        Console.WriteLine($"\t{s}");
        File.AppendAllText(path, $"{s}\n");
    }
    public static void CreateIfNotExists(this string path, string initialText = "")
    {
        if (!File.Exists(path))
            File.WriteAllText(path, "");
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

    public static void PrintPreview(this IDataView dataView, int maxRows = 100)
    {
        DataDebuggerPreview preview = dataView.Preview(maxRows);
        Console.WriteLine($"{maxRows}\t{preview.ColumnView.Select(x => x.Column.Name).Aggregate((x, y) => $"{x}\t{y}")}");
        int ct = 0;
        foreach (DataDebuggerPreview.RowInfo row in preview.RowView)
        {
            Console.Write($"{ct++}");
            foreach (object value in row.Values.Select(x => x.Value))
            {
                if (value is IEnumerable enumerable)
                {
                    foreach (object item in enumerable)
                    {
                        Console.Write($"\t{item}");
                    }
                }
                else
                {
                    Console.Write($"\t{value}");
                }
            }
            Console.WriteLine();
            if (ct > maxRows)
                break;
        }
    }
}
