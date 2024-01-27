using d9.utl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames;
public interface IMarkovProcess<TContext, TElement>
    where TContext : notnull, IEnumerable<TElement>
    where TElement : notnull
{
    protected Dictionary<TContext, CountingDictionary<TElement, int>> Data { get; set; }
    public TElement? NextElement(TContext? context = default);
    public TContext Preprocess(TContext item) => item;
    public void Add(TContext item)
    {
        if (Data is null)
            Data = new();
        Preprocess(item);
        
        for (int i = 0; i < s.Length; i++)
        {
            if (!Data.ContainsKey(cur))
                Data[cur] = new();
            // Console.WriteLine($"{"".PadLeft(i)}{cur}");
            Data[cur].Increment(s[i]);
            cur = s[i];
        }
    }
}
