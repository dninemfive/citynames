namespace citynames;
/// <summary>
/// Represents a class which can generate a random string and save itself.
/// </summary>
/// <typeparam name="T">A type representing the metadata which provides specifications to which the
///                     model should conform when generating strings.</typeparam>
public interface ISaveableStringGenerator<T>
{
    /// <summary>
    /// Generates a random string between <paramref name="minLength"/> and <paramref name="maxLength"/>,
    /// subject to specifics given by <paramref name="query"/>.
    /// </summary>
    /// <param name="query">Metadata to be used by the string generator to reflect in its output.</param>
    /// <param name="minLength">The minimum length, <b>inclusive</b>, of the resulting string.</param>
    /// <param name="maxLength">The maximum length, <b>inclusive</b>, of the resulting string.</param>
    /// <returns>A random string as described above.</returns>
    public string RandomString(T query, int minLength, int maxLength);
    /// <summary>
    /// Saves the string generator to the specified <paramref name="path"/> so that it can (ideally)
    /// be loaded later.
    /// </summary>
    /// <param name="path">The path to which to save the string generator.</param>
    /// <returns><see cref="Task.CompletedTask"/> when saving is complete.</returns>
    public Task SaveAsync(string path);
}