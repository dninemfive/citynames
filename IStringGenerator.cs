namespace citynames;
public interface IStringGenerator
{
    public string RandomString { get; }
    public string RandomStringOfLength(int min, int max, int maxAttempts);
}
