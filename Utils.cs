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
    public static void CreateIfNotExists(this string path)
    {
        if (!File.Exists(path))
            File.CreateText(path);
    }
}
