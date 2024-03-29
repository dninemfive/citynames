using d9.utl;
using System.Numerics;

namespace citynames;
public static class CollectionExtensions
{
    public static int Argmax<T>(this IEnumerable<T> items)
        where T : IComparisonOperators<T, T, bool>
    {
        T max = items.First();
        int result = 0;
        for (int i = 1; i < items.Count(); i++)
            if (items.ElementAt(i) > max)
                (max, result) = (items.ElementAt(i), i);
        return result;
    }
    public static int Argrand(this IEnumerable<float> items)
    {
        IEnumerable<int> indices = 0.To(items.Count());
        return indices.Zip(items).WeightedRandomElement(t => t.Second).First;
    }
    public static IEnumerable<(T, U)> CrossJoin<T, U>(this IEnumerable<T> ts, IEnumerable<U> us)
        => ts.SelectMany(t => us.Select(u => (t, u)));
    public static IEnumerable<T> To<T>(this T start, T end, T? step = null)
        where T : struct, INumberBase<T>, IComparisonOperators<T, T, bool>
    {
        step ??= T.One;
        Func<T, T> increment  = start < end ? x => x + step.Value
                                            : x => x - step.Value;
        Func<T, bool> compare = start < end ? x => x < end
                                            : x => x > end;
        T i = start;
        while (compare(i))
        {
            yield return i;
            i = increment(i);
        }
    }
}