namespace citynames;
/// <summary>
/// Various extension methods which make file operations more convenient.
/// </summary>
public static class FileUtilityExtensions
{
    /// <summary>
    /// Creates the specified file if it does not already exist.
    /// </summary>
    /// <param name="path">The path to the file to create.</param>
    /// <param name="initialText">The initial text to write to the path.</param>
    public static void CreateIfNotExists(this string path, string initialText = "")
    {
        if (!File.Exists(path))
            File.WriteAllText(path, initialText);
    }
    /// <summary>
    /// Converts the specified string to one which can be used in the filesystem.
    /// </summary>
    /// <param name="s">The string to convert.</param>
    /// <returns><paramref name="s"/>, but with each character which is not safe for the filesystem
    ///          replaced with its integer value surrounded by grave (<c>`</c>) characters.</returns>
    public static string FileNameSafe(this string s)
    {
        string result = string.Empty;
        foreach (char c in s)
            result += c.IsInvalidFileNameChar() ? $"`{(int)c}`" : c;
        return result;
    }
    /// <summary>
    /// Whether the specified character is safe for a file name on the current filesystem.
    /// </summary>
    /// <param name="c">The character whose safety to check.</param>
    /// <returns><see langword="true"/> if the character is safe as specified above, or
    ///          <see langword="false"/> otherwise.</returns>
    public static bool IsInvalidFileNameChar(this char c) => Path.GetInvalidFileNameChars().Contains(c);
}