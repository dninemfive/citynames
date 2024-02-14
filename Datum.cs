namespace citynames;
public class Datum
{
    public string Biome;
    public string Context;
    public char Result;
    public Datum(string biome, string context, char result)
    {
        Biome = biome;
        Context = context;
        Result = result;
    }
    public void Deconstruct(out string biome, out string context, out char result)
    {
        biome = Biome;
        context = Context;
        result = Result;
    }
    public string CsvString => $"{Biome},{Context},{Result}";
    public override string ToString()
        => $"{Biome}\t{Context}\t{Result}\t({(int)Result})";
    public override bool Equals(object? obj)
        => obj is Datum d && d.Biome == Biome && d.Context == Context && d.Result == Result;
    public override int GetHashCode()
        => HashCode.Combine(Biome, Context, Result);
    public static bool operator ==(Datum a, Datum b) => a.Equals(b);
    public static bool operator !=(Datum a, Datum b) => !(a == b);
}