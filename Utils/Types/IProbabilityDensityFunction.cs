namespace citynames;
public interface IProbabilityDensityFunction
{
    public Probability Between(double minValue, double maxValue)
        => GreaterThan(minValue).And(LessThan(maxValue));
    public Probability LessThan(double value);
    public Probability GreaterThan(double value);
}