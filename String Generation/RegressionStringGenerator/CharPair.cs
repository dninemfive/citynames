namespace citynames;
public class CharPair(char ancestor, char result, int offset, QueryInfo data)
{
    public readonly char Ancestor = ancestor, Result = result;
    // e.g. 1 = {ancestor}{result}, 2 = {ancestor}.{result}, 3 = {ancestor}..{result}
    public readonly int Offset = offset;
    public readonly QueryInfo Data = data;
    public static IEnumerable<CharPair> From(string cityName, CityInfo metadata, int maxOffset = 4)
    {
        Console.WriteLine(LogUtils.Method(args: [(nameof(cityName), cityName), (nameof(metadata), metadata)]));
        for (int i = 1; i < cityName.Length; i++)
        {
            char successor = cityName[i];
            for (int j = i - 1; j >= 0; j--)
            {
                char ancestor = cityName[j];
                int offset = i - j;
                Console.WriteLine($"\t{i},{j}: {ancestor}/{(int)ancestor},{successor}/{(int)successor}");
                if (offset > maxOffset)
                    break;
                yield return new(ancestor, successor, offset, new(metadata.Biome));
            }
        }
    }
    public static IEnumerable<CharPair> From((string cityName, CityInfo metadata) tuple, int maxOffset = 4)
        => From(tuple.cityName, tuple.metadata, maxOffset);
    public static IEnumerable<CharPair> From(IEnumerable<(string cityName, CityInfo metadata)> tuples, int maxOffset = 4)
        => tuples.SelectMany(x => From(x, maxOffset)).ToList();
}