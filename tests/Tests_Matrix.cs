namespace citynames.tests;
[TestClass]
public class Tests_Matrix
{
    [TestMethod]
    public void Test_Matrix_Constructor()
    {
        Matrix<double> matrix = new(new double[,]
        {
            { -1, 3/2.0 },
            { 1,     -1 }
        });
        Console.WriteLine(matrix);
    }
}
