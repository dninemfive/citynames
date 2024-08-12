using d9.utl;

namespace citynames;
public class ContinuousDistribution(CountingDictionary<double, int> counts, Func<double, double, double>? distanceFn = null)
{
    private readonly CountingDictionary<double, int> _counts = counts;
    private readonly Func<double, double, double> _distanceFn = distanceFn ?? ((x1, x2) => Math.Abs(x1 - x2));
    public double this[double value]
    {
        get
        {
            List<double> values = [];
            double totalWeight = 0;
            foreach((double x, int y) in _counts.Select(kvp => (kvp.Key, kvp.Value)))
            {
                double distance = _distanceFn(value, x).EnsureNonZero();
                totalWeight += distance;
                values.Add(distance * y);
            }
            return values.Sum() / totalWeight;
        }
    }
}