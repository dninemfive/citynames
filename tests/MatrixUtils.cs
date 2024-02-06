namespace citynames.tests;
[TestClass]
public class MatrixUtils
{
    [TestMethod]
    public void Dot()
    {
        Assert.AreEqual(15, new double[] { 1, 2, 3 }.Dot(new double[] { 4, -5, 7 }));
    }
}
