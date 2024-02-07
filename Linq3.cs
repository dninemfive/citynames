﻿using System.Numerics;

namespace citynames;
public static class Linq3
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
    public static IEnumerable<(T, U)> CrossJoin<T, U>(this IEnumerable<T> ts, IEnumerable<U> us)
        => ts.SelectMany(t => us.Select(u => (t, u)));
    public static T Dot<T>(this T[] a, T[] b)
        where T : INumberBase<T>
        => a.Zip(b)
            .Select(x => x.First * x.Second)
            .Sum();
    public static T Sum<T>(this IEnumerable<T> enumerable)
        where T : INumberBase<T>
        => enumerable.Aggregate((x, y) => x + y);
    public static IEnumerable<int> To(this int a, int b)
    {
        for (int i = a; i < b; i++)
            yield return i;
    }
}