﻿using d9.utl;
using Microsoft.ML.Data;
using System.Reflection;

namespace citynames;
public class NgramInfo(string context, string successor, string biome)
{
    [LoadColumn(0)]
    public string Context = context;
    [LoadColumn(1)]
    public string Successor = successor;
    [LoadColumn(2)]
    public string Biome = biome;

    public static NgramInfo Query(string biome)
        => new(string.Empty, string.Empty, biome);
    public void Deconstruct(out string context, out string successor, out string biome)
    {
        context = Context;
        successor = Successor;
        biome = Biome;
    }
    public static IEnumerable<FieldInfo> FieldsInColumnOrder
    {
        get
        {
            foreach (MemberInfo mi in typeof(NgramInfo).MembersWithAttribute<LoadColumnAttribute>().OrderBy(x => x.attr.TypeId).Select(x => x.member))
                if (mi is FieldInfo fi)
                    yield return fi;
        }
    }
    public static string CsvHeader => FieldsInColumnOrder.Select(x => x.Name).ListNotation(brackets: null, delimiter: ",");
    public string CsvLine() => FieldsInColumnOrder.Select(x => x.GetValue(this)).ListNotation(brackets: null, delimiter: ",");
    public override bool Equals(object? obj)
        => obj is NgramInfo d && d.Biome == Biome && d.Context == Context && d.Successor == Successor;
    public override int GetHashCode()
        => HashCode.Combine(Biome, Context, Successor);
    public static bool operator ==(NgramInfo a, NgramInfo b) => a.Equals(b);
    public static bool operator !=(NgramInfo a, NgramInfo b) => !(a == b);
}