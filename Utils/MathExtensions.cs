using System.Numerics;

namespace citynames;
/// <summary>
/// Extensions which make some math operations more convenient.
/// </summary>
public static class MathExtensions
{
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