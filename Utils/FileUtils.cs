namespace citynames;
public static class FileUtils
{
    public static void CreateIfNotExists(this string path, string initialText = "")
    {
        if (!File.Exists(path))
            File.WriteAllText(path, initialText);
    }
    public static string FileNameSafe(this string s)
    {
        string result = string.Empty;
        foreach (char c in s)
            result += c.IsInvalidFileNameChar() ? $"`{(int)c}`" : c;
        return result;
    }
    public static bool IsInvalidFileNameChar(this char c) => Path.GetInvalidFileNameChars().Contains(c);
}
