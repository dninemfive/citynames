namespace citynames;
public interface IStringGenerator<T>
{
    public string RandomString(T input);
    public string RandomStringOfLength(T input, int min = 1, int max = int.MaxValue, int maxAttempts = 100);
    public IEnumerable<string> RandomStringsOfLength(T input, int count, int min = 1, int max = int.MaxValue, int attemptsPer = 100)
    {
        for (int i = 0; i < count; i++)
            yield return RandomStringOfLength(input, min, max, attemptsPer);
    }
}
