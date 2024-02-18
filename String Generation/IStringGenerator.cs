namespace citynames;
public interface IStringGenerator<T>
{
    public string RandomString(T input);
}
public static class StringGeneratorExtensions
{
    public static string RandomStringOfLength<T>(this IStringGenerator<T> generator, T input, int min = 1, int max = int.MaxValue, int maxAttempts = 100)
    {
        string result = "";
        int ct = 0;
        while (result.Length < min || result.Length > max)
        {
            result = generator.RandomString(input);
            ;
            if (++ct == maxAttempts)
            {
                Console.WriteLine($"Failed to generate random string with target length [{min}..{max}] after {maxAttempts} attempts.");
                break;
            }
        }
        return result;
    }
    public static IEnumerable<string> RandomStringsOfLength<T>(this IStringGenerator<T> generator, 
                                                                    T input, 
                                                                    int count, 
                                                                    int minLength = 1, 
                                                                    int maxLength = int.MaxValue, 
                                                                    int maxAttemptsPerString = 100)
    {
        for (int i = 0; i < count; i++)
            yield return generator.RandomStringOfLength(input, minLength, maxLength, maxAttemptsPerString);
    } 
}
