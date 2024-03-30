namespace citynames;
/// <summary>
/// Represents a string generator which can be built from data and loaded from a saved file.
/// </summary>
/// <typeparam name="T"><inheritdoc cref="ISaveableStringGenerator{T}" path="/typeparam[@name='T']/node()"/></typeparam>
/// <typeparam name="TSelf">The type of the string generator itself.</typeparam>
public interface IBuildLoadableStringGenerator<T, TSelf> : ISaveableStringGenerator<T>
{
    /// <summary>
    /// Builds the string generator from a specified <paramref name="corpus"/> of strings and 
    /// associated metadata, given a specific <paramref name="contextLength"/>.
    /// </summary>
    /// <param name="corpus">
    ///     A collection of strings paired with metadata which should provide associations which
    ///     allow the string generator to vary its output in response to queries.
    /// </param>
    /// <param name="contextLength">The length of context n-grams.</param>
    /// <returns>A model built as described above.</returns>
    public static abstract TSelf Build(IEnumerable<(string item, T metadata)> corpus, int contextLength = 2);
    /// <summary>
    /// Loads the string generator from a saved file.
    /// </summary>
    /// <param name="path">The path to the file from which to load the string generator.</param>
    /// <returns>The loaded string generator.</returns>
    public static abstract TSelf Load(string path);
}