namespace citynames.tests;
[TestClass]
public class Tests_Matrix
{
    private List<double[,]> _testMatricesA = new List<double[,]>()
    {
        // 2x2
        new double[,] {
            {  1,  2 },
            { -3, -4 }
        },
        // 3x2
        new double[,] {
            {  1,  2 },
            { -3,  4 },
            { -5, -6 }
        },
        // 2x3
        new double[,] {
            {  1,  2,  3 },
            { -4, -5, -6 }
        },
    };
    private List<double[,]> _testMatricesB = new List<double[,]>()
    {
        // 2x2
        new double[,] {
            { 1, -2 },
            { 3, -4 }
        },
        // 3x2
        new double[,] {
            {  1, -2 },
            {  3, -4 },
            {  5, -6 }
        },
        // 2x3
        new double[,] {
            {  1, -2,  3 },
            {  4, -5, -6 }
        }
    };
    [TestMethod]
    public void Test_ToString()
    {
        foreach (Matrix<double> matrix in _testMatricesA)
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
        foreach(Matrix<double> testMatrix in _testMatricesA)
        {
            Matrix<double> actual = -testMatrix;
            foreach((int r, int c) in testMatrix.Cells)
                Assert.AreEqual(-testMatrix[r, c], actual[r, c]);
        }
    }
    [TestMethod]
    public void Test_Addition()
    {
        Assert.ThrowsException<ArgumentException>(() => new Matrix<double>(_testMatricesA[0]) + new Matrix<double>(_testMatricesB[1]));
        foreach((Matrix<double> a, Matrix<double> b) in _testMatricesA.Zip(_testMatricesB))
        {
            Matrix<double> actual1 = a + b, actual2 = b + a;
            foreach ((int r, int c) in a.Cells)
            {
                Assert.AreEqual(a[r, c] + b[r, c], actual1[r, c]);
                Assert.AreEqual(a[r, c] + b[r, c], actual2[r, c]);
            }
        }
    }
    [TestMethod]
    public void Test_Subtraction()
    {
        Assert.ThrowsException<ArgumentException>(() => new Matrix<double>(_testMatricesA[0]) - new Matrix<double>(_testMatricesB[1]));
        foreach ((Matrix<double> a, Matrix<double> b) in _testMatricesA.Zip(_testMatricesB))
        {
            Matrix<double> actual1 = a - b, actual2 = b - a;
            foreach ((int r, int c) in a.Cells)
            {
                Assert.AreEqual(a[r, c] - b[r, c], actual1[r, c]);
                Assert.AreEqual(b[r, c] - a[r, c], actual2[r, c]);
            }
        }
    }
    [TestMethod]
    public void Test_ScalarMultiplication()
    {
        foreach(Matrix<double> matrix in _testMatricesA)
        {
            Matrix<double> actual1 = matrix * 2, actual2 = 2 * matrix;
            foreach ((int r, int c) in matrix.Cells)
            {
                Assert.AreEqual(matrix[r, c] * 2, actual1[r, c]);
                Assert.AreEqual(matrix[r, c] * 2, actual2[r, c]);
            }
        }
    }
    [TestMethod]
    public void Test_MatrixMultiplication()
    {
        Assert.ThrowsException<ArgumentException>(() => new Matrix<double>(_testMatricesA[0]) * new Matrix<double>(_testMatricesB[1]));
        Assert.ThrowsException<ArgumentException>(() => new Matrix<double>(_testMatricesA[1]) * new Matrix<double>(_testMatricesB[1]));
        List<Matrix<double>> actualMatrices = new()
        {
            _testMatricesA[0].ToMatrix() * _testMatricesB[0],
            _testMatricesA[1].ToMatrix() * _testMatricesB[2],
            _testMatricesA[2].ToMatrix() * _testMatricesB[1],
        };
        List<(int a, int b)> indices = new() { (0, 0), (1, 2), (2, 1) };
        foreach((int i, int j) in indices)
        {
            Matrix<double> a = _testMatricesA[i], b = _testMatricesB[j];
            Matrix<double> actual = a * b;
            Assert.AreEqual(actual.Dimensions, (a.RowCount, b.ColumnCount));
            foreach((int r, int c) in actual.Cells)
            {
                double expected = a.Row(r)
                                   .Zip(b.Column(c))
                                   .Select(x => x.First * x.Second)
                                   .Sum();
                Assert.AreEqual(expected, actual[r, c]);
            }
        }
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
