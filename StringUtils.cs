using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames;
public static class StringUtils
{
    public static string SubstringSafe(this string str, int start, int end)
    {
        start = Math.Max(start, 0);
        end = Math.Min(end, str.Length);
        return str[start..end];
    }
    public static string Last(this string str, int n)
        => SubstringSafe(str, str.Length - n, str.Length);
}
