using System.Numerics;

namespace citynames.Utils;

public static class MathExtensions
{
    public static T Dot<T>(this IEnumerable<T> a, IEnumerable<T> b)
        where T : INumberBase<T>
        => a.Zip(b)
            .Select(x => x.First * x.Second)
            .Sum();
    public static T Sum<T>(this IEnumerable<T> enumerable)
        where T : IAdditionOperators<T, T, T>
        => enumerable.Aggregate((x, y) => x + y);
}