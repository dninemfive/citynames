using Microsoft.ML.Data;

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
    public override bool Equals(object? obj)
        => obj is NgramInfo d && d.Biome == Biome && d.Context == Context && d.Successor == Successor;
    public override int GetHashCode()
        => HashCode.Combine(Biome, Context, Successor);
    public static bool operator ==(NgramInfo a, NgramInfo b) => a.Equals(b);
    public static bool operator !=(NgramInfo a, NgramInfo b) => !(a == b);
}