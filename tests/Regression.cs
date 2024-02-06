using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames.tests;
[TestClass]
public class Regression
{
    [TestMethod]
    public void RegressionCoefficients()
    {
        Matrix<double> Xs = new double[,]
        {
            { 0.589158461, 0.16627127, 0.869848776 },
            { 0.206584249, 0.742638638, 0.486461702 },
            { 0.337796867, 0.906499363, 0.623294137 },
            { 0.318072672, 0.764272938, 0.549785385 },
            { 0.197850346, 0.46843904, 0.135128625 },
            { 0.044927907, 0.876269945, 0.591140505 },
            { 0.48963664, 0.189952838, 0.697937769 },
            { 0.003636324, 0.065149833, 0.453915431 },
            { 0.60445273, 0.484182672, 0.478406222 },
            { 0.249049575, 0.327568604, 0.306744612 }
        };
        Matrix<double> Ys = new double[,]
        {
            { 0.990673944 },
            { 0.698666318 },
            { 0.336576607 },
            { 0.210232186 },
            { 0.604904741 },
            { 0.491183787 },
            { 0.800983259 },
            { 0.581583005 },
            { 0.392699712 },
            { 0.618491819 }
        };
        Console.WriteLine(Xs.Transposition);
        Matrix<double> m = Xs * Xs.Transposition;
        Console.WriteLine(m);
        Console.WriteLine(m.Dimensions);
        Console.WriteLine(m * m.Transposition);
        Console.WriteLine(citynames.Regression.RegressionCoefficients(Xs, Ys));
    }
}
