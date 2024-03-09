namespace citynames;
public interface ISaveableStringGenerator<T>
{
    public string RandomString(T input, int minLength, int maxLength);
    public Task SaveAsync(string path);
}