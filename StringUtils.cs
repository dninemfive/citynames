using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames;
public static class StringUtils
{
    public static string SubstringSafe(this string str, int length = 1, int endIndex = int.MaxValue)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(length, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(endIndex);
        return str.Substring(Math.Max(endIndex - length, 0), Math.Min(endIndex, str.Length - 1));
    }
}
