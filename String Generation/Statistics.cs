using d9.utl;
using System.Numerics;

namespace citynames;
internal static class Statistics
{
    public static bool IsValidProbability<T>(this T x)
        where T : IFloatingPoint<T>
        => x >= T.Zero && x <= T.One;
    public static void AssertValidProbability<T>(T p, string name)
        where T : IFloatingPoint<T>
        => AssertValidProbabilities((p, name));
    public static void AssertValidProbabilities<T>(params (T p, string name)[] probabilities)
        where T : IFloatingPoint<T>
    {
        string invalidNames = probabilities.Where(x => !x.p.IsValidProbability())
                                           .Select(x => $"\"{x.name}\" ({x.p})")
                                           .NaturalLanguageList(conjunction: "and");
        if (!invalidNames.NullOrEmpty())
            throw new ArgumentOutOfRangeException($"{invalidNames} must be in the range [0..1]!");
    }
    public static T PosteriorProbability<T>(T prior, T marginal, T bGivenA)
        where T : IFloatingPoint<T>
    {
        AssertValidProbabilities((prior, "P(A)"), (marginal, "P(B)"), (bGivenA, "P(B|A)"));
        return prior * bGivenA / marginal;
    }
    public static T Lerp<T>(T a, T b, T aWeight)
        where T : IFloatingPoint<T>
    {
        AssertValidProbability(aWeight, nameof(aWeight));
        return a * aWeight + (T.One - aWeight) * b;
    }
    public static float StandardDeviation(this IEnumerable<float> items)
        => (float)Math.Sqrt(items.Average(x => Math.Pow(x - items.Average(), 2)));
}