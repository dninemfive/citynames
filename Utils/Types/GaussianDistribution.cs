namespace citynames;

public class GaussianDistribution(double mean, double standardDeviation) : IProbabilityDensityFunction
{
    private static readonly double _sqrt2Pi = Math.Sqrt(2 * Math.PI);
    public double Mean { get; private set; } = mean;
    public double StandardDeviation { get; private set; } = standardDeviation;
    public Probability this[double x]
        => (1 / (StandardDeviation * _sqrt2Pi)) * Math.Exp(-0.5 * Math.Pow((x - Mean) / StandardDeviation, 2));
    public Probability LessThan(double x)
        => 0.5 * (1 + StatisticsUtils.Erf(x / Math.Sqrt(2)));
    public Probability GreaterThan(double x) => 1 - LessThan(x);
}