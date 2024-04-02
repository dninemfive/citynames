namespace citynames;
public readonly struct Probability(double d)
{
    public readonly double Value = (d is >= 0 and <= 1) ? d : throw new ArgumentOutOfRangeException(nameof(d));
    public static implicit operator Probability(double d) => new(d);
    public static implicit operator double(Probability p) => p.Value;
    public Probability Inverse => 1 - this;
    public Probability Or(Probability other, Probability? pBoth = null)
        => Math.Clamp(this + other - (pBoth ?? 0), 0, 1);
    public Probability And(Probability pOtherGivenThis)
        => this * pOtherGivenThis;
    public Probability Given(Probability other, Probability? pOtherGivenThis = null)
        => this * (pOtherGivenThis ?? 1) / other;
}