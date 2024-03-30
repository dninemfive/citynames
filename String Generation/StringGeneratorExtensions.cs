namespace citynames;
public static class StringGeneratorExtensions
{
    /// <summary>
    /// Generates a random string of length between <paramref name="min"/> and
    /// <paramref name="max"/>, inclusive, matching the specified <paramref name="input"/> and
    /// trying up to <paramref name="maxAttempts"/> times to meet the specified length criteria.
    /// </summary>
    /// <typeparam name="T">The type of the query data to the generator.</typeparam>
    /// <param name="generator">The string generator to use to generate the string.</param>
    /// <param name="input">The query to the string generator.</param>
    /// <param name="min">The minimum length of the resulting string.</param>
    /// <param name="max">The maximum length of the resulting string.</param>
    /// <param name="maxAttempts">The maximum number of attempts to make before giving up.</param>
    /// <returns>
    ///     A string which may have length between <paramref name="min"/> and <paramref name="max"/>,
    ///     but this is not guaranteed because <paramref name="maxAttempts"/> may be exceeded.
    /// </returns>
    public static string RandomStringOfLength<T>(this ISaveableStringGenerator<T> generator,
                                                      T input,
                                                      int min = 1,
                                                      int max = int.MaxValue,
                                                      int maxAttempts = 100)
    {
        string result = "";
        int ct = 0;
        while (!result.Length.Between(min, max))
        {
            result = generator.RandomString(input, min, max);
            if (++ct >= maxAttempts)
            {
                Console.WriteLine($"Failed to generate random string with target length [{min}..{max}] after {maxAttempts} attempts.");
                break;
            }
        }
        return result;
    }
    public static IEnumerable<string> RandomStringsOfLength<T>(this ISaveableStringGenerator<T> generator,
                                                                    T input,
                                                                    int count,
                                                                    int minLength = 1,
                                                                    int maxLength = int.MaxValue,
                                                                    int maxAttemptsPerString = 100)
    {
        for (int _ = 0; _ < count; _++)
            yield return generator.RandomStringOfLength(input, minLength, maxLength, maxAttemptsPerString);
    }
}