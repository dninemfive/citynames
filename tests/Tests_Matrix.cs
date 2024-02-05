namespace citynames.tests;
[TestClass]
public class Tests_Matrix
{
    [TestMethod]
    public void Test_Constructor()
    {
        Matrix<double> matrix = new(new double[,]
        {
            { -1, 3/2.0 },
            { 1,     -1 }
        });
        Console.WriteLine(matrix);
    }
    [TestMethod]
    public void Test_Equals()
    {
        Matrix<double> original = new(new double[,]
        {
            { -1, 3/2.0 },
            { 1,     -1 }
        });
        Matrix<double> other = new(new double[,]
        {
            { -1, 3/2.0 },
            { 1,     -1 }
        });
        Assert.AreEqual(original, other);
    }
    [TestMethod]
    public void Test_UnaryNegation()
    {
        Matrix<double> original = new(new double[,]
        {
            { -1, 3/2.0 },
            { 1,     -1 }
        });
        Matrix<double> expected = new(new double[,]
        {
            { 1, -3.0/2.0 },
            { -1,     1 }
        });
        Assert.AreEqual(expected, -original);
    }
    [TestMethod]
    public void Test_Addition()
    {
        Assert.ThrowsException<ArgumentException>(() => new Matrix<double>(new double[,]
        {
            { 5, 4 }
        }) + new Matrix<double>(new double[,]
        {
            { 4, 6 }, { 5, 2}
        }));
        Matrix<double> a = new(new double[,]
        {
            { 1, 2 }, 
            { -3, -4 }
        });
        Matrix<double> b = new(new double[,]
        {
            { 5, -3 }, 
            { 6, -7 }
        });
        Matrix<double> expected = new(new double[,]
        {
            { 6, -1 }, { 3, -11 }
        });
        Assert.AreEqual(expected, a + b);
        Assert.AreEqual(expected, b + a);
    }
    [TestMethod]
    public void Test_Subtraction()
    {
        Assert.ThrowsException<ArgumentException>(() => new Matrix<double>(new double[,]
        {
            { 5, 4 }
        }) - new Matrix<double>(new double[,]
        {
            { 4, 6 }, { 5, 2}
        }));
        Matrix<double> a = new(new double[,]
        {
            { 1, 2 },
            { -3, -4 }
        });
        Matrix<double> b = new(new double[,]
        {
            { 5, -3 },
            { 6, -7 }
        });
        Matrix<double> expected1 = new(new double[,]
        {
            { -4, 5 }, 
            { -9, 3 }
        });
        Assert.AreEqual(expected1, a - b);
        Matrix<double> expected2 = new(new double[,]
        {
            { 4, -5 },
            { 9, -3 }
        });
        Assert.AreEqual(expected2, b - a);
    }
    [TestMethod]
    public void Test_Identity()
    {
        Matrix<double> actual = Matrix<double>.Identity(2);
        Matrix<double> expected = new(new double[,]
        {
            { 1, 0 }, { 0, 1 }
        });
        Assert.AreEqual(expected, actual);
    }
}
