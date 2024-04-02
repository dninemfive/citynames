using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames.Utils;
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
public interface IProbabilityDensityFunction
{
    public Probability Between(double minValue, double maxValue)
        => GreaterThan(minValue).And(LessThan(maxValue));
    public Probability LessThan(double value);
    public Probability GreaterThan(double value);
}
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
public static class StatisticsUtils
{
    private static readonly double WinitzkiA = (8 * (Math.PI - 3)) / (3 * Math.PI * (4 - Math.PI));
    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    /// <remarks>Uses Winitzki's formula given <see href="https://en.wikipedia.org/wiki/Error_function#Numerical_approximations">here</see>.</remarks>
    public static double Erf(double x)
    {
        double xSquared = x * x,
               aXSquared = WinitzkiA * xSquared,
               upperTerm = 4 / Math.PI + aXSquared,
               lowerTerm = 1 + aXSquared,
               fraction = upperTerm / lowerTerm;
        return Math.Sign(x) * Math.Sqrt(1 - Math.Exp(-xSquared * fraction));
    }
}