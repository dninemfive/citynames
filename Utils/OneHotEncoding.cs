using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames.Utils;
public class OneHotEncoding<T>(IEnumerable<T> items)
    where T : IEquatable<T>
{
    private readonly List<T> _alphabet = [.. items.Distinct().Order()];
    public double[] Encode(T item)
    {
        if (!_alphabet.Contains(item))
            throw new ArgumentOutOfRangeException(nameof(item));
        double[] result = new double[_alphabet.Count];
        for (int i = 0; i < result.Length; i++)
            result[i] = item.Equals(_alphabet[i]) ? 1 : 0;
        return result;
    }
}
