namespace citynames;
public static class StatisticsUtils
{
    private static readonly double WinitzkiA = (8 * (Math.PI - 3)) / (3 * Math.PI * (4 - Math.PI));
    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    /// <remarks>Uses Winitzki's formula given <see href="">here</see>.</remarks>
    public static double Erf(double x)
    {
        double xSquared = x * x,
               aXSquared = WinitzkiA * xSquared,
               upperTerm = 4 / Math.PI + aXSquared,
               lowerTerm = 1 + aXSquared,
               fraction = upperTerm / lowerTerm;
        return Math.Sign(x) * Math.Sqrt(1 - Math.Exp(-xSquared * fraction));
    }
    public static double Lerp(double a, double b, double x)
        => (a * x) + (b * (1 - x));
    public static double ErfInterpolate(this double prior, double posterior, double x)
    {
        double erf = Erf(Math.Abs(x));
        return prior * (1 - erf) + posterior * erf;
    }
}