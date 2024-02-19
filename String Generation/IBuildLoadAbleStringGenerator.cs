using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames;
public interface IBuildLoadAbleStringGenerator<T, TSelf> : ISaveableStringGenerator<T>
{
    public static abstract TSelf Build(IEnumerable<T> input, int contextLength = 2);
    public static abstract TSelf Load(string path);
}