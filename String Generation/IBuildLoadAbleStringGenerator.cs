namespace citynames;
public interface IBuildLoadableStringGenerator<T, TSelf> : ISaveableStringGenerator<T>
{
    public static abstract TSelf Build(IEnumerable<T> input, int contextLength = 2);
    public static abstract TSelf Load(string path);
}