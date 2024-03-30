using System.Numerics;

namespace citynames;
/// <summary>
/// Extensions which make some math operations more convenient.
/// </summary>
public static class MathExtensions
{
    /// <summary>
    /// Returns whether <paramref name="t"/> is between <paramref name="min"/> and 
    /// <paramref name="max"/>. If <paramref name="min"/> and <paramref name="max"/> are out of
    /// order, this is corrected before continuing.
    /// </summary>
    /// <typeparam name="T">The type of item to compare.</typeparam>
    /// <param name="t">The item whose value to check.</param>
    /// <param name="min">The minimum acceptable value, which may or may not be inclusive depending
    ///                   on the value of the <paramref name="inclusive"/> argument.</param>
    /// <param name="max">The maximum acceptable value, which may or may not be inclusive depending
    ///                   on the value of the <paramref name="inclusive"/> argument.</param>
    /// <param name="inclusive">
    ///     If <see langword="true"/>, the result will be true if <paramref name="t"/> is equal
    ///     to either <paramref name="min"/> or <paramref name="max"/>; otherwise, 
    ///     <paramref name="t"/> must be strictly greater than <paramref name="min"/> and strictly
    ///     less than <paramref name="max"/>.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if <paramref name="t"/> is between <paramref name="min"/> and 
    ///     <paramref name="max"/>, as modified by <paramref name="inclusive"/>; otherwise,
    ///     <see langword="false"/>.
    /// </returns>
    public static bool Between<T>(this T t, T min, T max, bool inclusive = true)
        where T : IComparisonOperators<T, T, bool>
    {
        if (min > max)
            (min, max) = (max, min);
        return inclusive ? t >= min && t <= max
                         : t >  min && t < max;
    }
    /// <summary>
    /// Produces the <see href="https://en.wikipedia.org/wiki/Dot_product">dot product</see> of two
    /// specified collections of numbers.
    /// </summary>
    /// <typeparam name="T">The type of the numbers whose dot product to calculate.</typeparam>
    /// <param name="a">The left-hand collection of numbers.</param>
    /// <param name="b">The right-hand collection of numbers.</param>
    /// <returns>The dot product of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static T Dot<T>(this IEnumerable<T> a, IEnumerable<T> b)
        where T : INumberBase<T>
        => a.Zip(b)
            .Select(x => x.First * x.Second)
            .Sum();
    /// <summary>
    /// Generalization of LINQ's <c>.Sum()</c> to any type which defines addition operators
    /// between and producing itself.
    /// </summary>
    /// <typeparam name="T">The type of the addends and of the summand.</typeparam>
    /// <param name="enumerable">The collection of items to sum.</param>
    /// <returns>The sum of the items of <paramref name="enumerable"/>, as defined by 
    ///          <typeparamref name="T"/>.</returns>
    public static T Sum<T>(this IEnumerable<T> enumerable)
        where T : IAdditionOperators<T, T, T>
        => enumerable.Aggregate((x, y) => x + y);
}