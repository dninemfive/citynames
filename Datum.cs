namespace citynames;
public class Datum
{
    public string Biome;
    public string Context;
    public char Successor;
    public Datum(string biome, string context, char successor)
    {
        Biome = biome;
        Context = context;
        Successor = successor;
    }
    public void Deconstruct(out string biome, out string context, out char successor)
    {
        biome = Biome;
        context = Context;
        successor = Successor;
    }
    public static string CsvHeader => "biome,context,successor";
    public string CsvLine => $"{Biome},{Context},{Successor}";
    public override string ToString()
        => $"{Biome}\t{Context}\t{Successor}\t({(int)Successor})";
    public override bool Equals(object? obj)
        => obj is Datum d && d.Biome == Biome && d.Context == Context && d.Successor == Successor;
    public override int GetHashCode()
        => HashCode.Combine(Biome, Context, Successor);
    public static bool operator ==(Datum a, Datum b) => a.Equals(b);
    public static bool operator !=(Datum a, Datum b) => !(a == b);
}