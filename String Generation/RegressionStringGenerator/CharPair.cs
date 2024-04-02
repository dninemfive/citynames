﻿namespace citynames;
public class CharPair(char ancestor, char result, int offset, QueryInfo data)
{
    public readonly char Ancestor = ancestor, Result = result;
    // e.g. 1 = {ancestor}{result}, 2 = {ancestor}.{result}, 3 = {ancestor}..{result}
    public readonly int Offset = offset;
    public readonly QueryInfo Data = data;
    public static IEnumerable<CharPair> From(string cityName, string biome, int maxOffset = 4)
    {
        for (int i = 0; i < cityName.Length; i++)
        {
            char ancestor = cityName[i];
            for (int j = i + 1; i < cityName.Length; i++)
            {
                char successor = cityName[j];
                int offset = j - i;
                if (offset > maxOffset)
                    break;
                yield return new(ancestor, successor, offset, new(biome));
            }
        }
    }
    public static IEnumerable<CharPair> From((string cityName, string biome) tuple, int maxOffset = 4)
        => From(tuple.cityName, tuple.biome, maxOffset);
    public static IEnumerable<CharPair> From(IEnumerable<(string cityName, string biome)> tuples, int maxOffset = 4)
        => tuples.SelectMany(x => From(x, maxOffset)).ToList();
}