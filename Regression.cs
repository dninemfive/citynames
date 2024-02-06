using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames;
public class Regression
{
    private readonly Dictionary<string, double> _coefs = new();
    private double _const = 0;
    public Regression(params string[] variableNames)
    {
        foreach (string variableName in variableNames)
            _coefs[variableName] = default;
    }
    public void RegressOn(Dictionary<string, double[]> data)
    {
        
    }
    public static Matrix<double> RegressionCoefficients(Matrix<double> X, Matrix<double> Y)
        => (X.Transposition * X).Inverse! * X.Transposition * Y;
    public double Predict(Dictionary<string, double> data)
    {
        double result = _const;
        foreach((string key, double value) in _coefs)
            result += value * _coefs[key];
        return result;
    }
}
