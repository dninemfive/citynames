using d9.utl;
using System.Numerics;

namespace citynames;
/// <summary>
/// Various extensions to <see cref="IEnumerable{T}"/> which make manipulating collections more
/// convenient.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Finds the index of the maximum value in <paramref name="items"/>.
    /// </summary>
    /// <typeparam name="T">The type of the items in <paramref name="items"/>.</typeparam>
    /// <param name="items">A collection of items which implement comparison operators.</param>
    /// <returns>The index of the item in <paramref name="items"/> with the greatest value.
    ///          If there are multiple such items, returns the index of the first.</returns>
    public static int IndexOfMaxValue<T>(this IEnumerable<T> items)
        where T : IComparisonOperators<T, T, bool>
    {
        T max = items.First();
        int result = 0;
        for (int i = 1; i < items.Count(); i++)
            if (items.ElementAt(i) > max)
                (max, result) = (items.ElementAt(i), i);
        return result;
    }
    /// <summary>
    /// Returns the index of a random element in <paramref name="items"/>, weighted by their values.
    /// </summary>
    /// <param name="items">The items whose random index to obtain.</param>
    /// <returns>A random index of the <paramref name="items"/>, with each index having a probability
    ///          equal to the proportional weight of its corresponding item.</returns>
    public static int WeightedRandomIndex(this IEnumerable<float> items)
    {
        IEnumerable<int> indices = 0.To(items.Count());
        return indices.Zip(items).WeightedRandomElement(t => t.Second).First;
    }
    /// <summary>
    /// Produces a <see href="https://en.wikipedia.org/wiki/Join_(SQL)#Cross_join">cross join</see>
    /// of collections <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <typeparam name="T">The type of the items in collection <paramref name="a"/>.</typeparam>
    /// <typeparam name="U">The type of the items in collection <paramref name="b"/>.</typeparam>
    /// <param name="a">The first collection to cross join.</param>
    /// <param name="b">The second collection to cross join.</param>
    /// <returns>A collection of tuples representing the Cartesian product of the specified collections,
    ///          where every element of <paramref name="a"/> is paired with every element of 
    ///          <paramref name="b"/>.</returns>
    public static IEnumerable<(T, U)> CrossJoin<T, U>(this IEnumerable<T> a, IEnumerable<U> b)
        => a.SelectMany(t => b.Select(u => (left: t, right: u)));
    /// <summary>
    /// Produces a sequence of numbers in the range [<paramref name="start"/>,
    /// <paramref name="end"/>), stepping by <paramref name="step"/> between each element.
    /// </summary>
    /// <typeparam name="T">The type of the numbers to enumerate.</typeparam>
    /// <param name="start">The value, inclusive, at which the sequence starts.</param>
    /// <param name="end">The value, exclusive, at which the sequence ends.</param>
    /// <param name="step">The size of the steps to take.</param>
    /// <returns>A sequence of numbers which starts at <paramref name="start"/> and contains every
    /// <paramref name="step"/>th number until, but not including, <paramref name="end"/>.</returns>
    /// <remarks>If <paramref name="start"/> is greater than <paramref name="end"/>, the sequence
    ///          will count down. The parity of <paramref name="step"/> is irrelevant; only its
    ///          absolute value is used.</remarks>
    public static IEnumerable<T> To<T>(this T start, T end, T? step = null)
        where T : struct, INumberBase<T>, IComparisonOperators<T, T, bool>
    {
        step ??= T.One;
        T absoluteStep = T.IsNegative(step.Value) ? -step.Value : step.Value;
        Func<T, T> increment  = start < end ? x => x + absoluteStep
                                            : x => x - absoluteStep;
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