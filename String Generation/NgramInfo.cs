using d9.utl;
using Microsoft.ML.Data;
using System.Reflection;

namespace citynames;
public class NgramInfo
{
    [LoadColumn(0)]
    public string Context;
    [LoadColumn(1)]
    public char Successor;
    [LoadColumn(2)]
    public string Biome;
    public NgramInfo(string biome) : this(string.Empty, Constants.NullCharacter, biome) { }
    public NgramInfo(string context, char successor, string biome)
    {
        Context = context;
        Successor = successor;
        Biome = biome;
    }
    public void Deconstruct(out string context, out char successor, out string biome)
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
    public static string CsvHeader => FieldsInColumnOrder.Select(x => x.Name).JoinWithDelim(",");
    public string CsvLine => FieldsInColumnOrder.Select(x => x.GetValue(this)).JoinWithDelim(",");
    public override bool Equals(object? obj)
        => obj is NgramInfo d && d.Biome == Biome && d.Context == Context && d.Successor == Successor;
    public override int GetHashCode()
        => HashCode.Combine(Biome, Context, Successor);
    public static bool operator ==(NgramInfo a, NgramInfo b) => a.Equals(b);
    public static bool operator !=(NgramInfo a, NgramInfo b) => !(a == b);
}