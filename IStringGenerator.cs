namespace citynames;
public interface IStringGenerator<T>
{
    public string RandomString(T input);
    public string RandomStringOfLength(T input, int min = 1, int max = int.MaxValue, int maxAttempts = 100);
}
