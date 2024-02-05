using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames.tests;
[TestClass]
public class Tests_MatrixUtils
{
    [TestMethod]
    public void Test_Dot()
    {
        Assert.AreEqual(15, new double[] { 1, 2, 3 }.Dot(new double[] { 4, -5, 7 }));
    }
}
