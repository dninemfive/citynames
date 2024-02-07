namespace citynames.tests;
[TestClass]
public class Matrix
{
    public static class TestMatrices
    {
        public static IEnumerable<Matrix<double>> List
        {
            get
            {
                yield return _2x2_A;
                yield return _3x2_A;
                yield return _2x3_A;
            }
        }
        public static IEnumerable<(Matrix<double>, Matrix<double>)> Pairs
        {
            get
            {
                yield return (_2x2_A, _2x2_B);
                yield return (_3x2_A, _3x2_B);
                yield return (_2x3_A, _2x3_B);
            }
        }
        public static readonly Matrix<double> _2x2_A = new double[,] {
            {  1,  2 },
            { -3, -4 }
        };
        public static readonly Matrix<double> _2x2_B = new double[,] {
            { 1, -2 },
            { 3, -4 }
        };
        public static readonly Matrix<double> _3x2_A = new double[,] {
            {  1,  2 },
            { -3,  4 },
            { -5, -6 }
        };
        public static readonly Matrix<double> _3x2_B = new double[,] {
            {  1, -2 },
            {  3, -4 },
            {  5, -6 }
        };
        public static readonly Matrix<double> _2x3_A = new double[,] {
            {  1,  2,  3 },
            { -4, -5, -6 }
        };
        public static readonly Matrix<double> _2x3_B = new double[,] {
            {  1, -2,  3 },
            {  4, -5, -6 }
        };
    }
    [TestMethod]
    public void EqualsOperator()
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
    public void UnaryNegation()
    {
        foreach (Matrix<double> testMatrix in TestMatrices.List)
        {
            Matrix<double> actual = -testMatrix;
            foreach((int r, int c) in testMatrix.Cells)
                Assert.AreEqual(-testMatrix[r, c], actual[r, c]);
        }
    }
    [TestMethod]
    public void Addition_BoundsCheck()
    {
        Assert.ThrowsException<ArgumentException>(() => new Matrix<double>(TestMatrices._2x2_A) + new Matrix<double>(TestMatrices._3x2_A));
    }
    [TestMethod]
    public void Addition_Correctness()
    {
        foreach((Matrix<double> a, Matrix<double> b) in TestMatrices.Pairs)
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
    public void Subtraction_BoundsCheck()
    {
        Assert.ThrowsException<ArgumentException>(() => new Matrix<double>(TestMatrices._2x2_A) - new Matrix<double>(TestMatrices._3x2_A));
    }
    [TestMethod]
    public void Subtraction_Correctness()
    {
        foreach ((Matrix<double> a, Matrix<double> b) in TestMatrices.Pairs)
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
    public void ScalarMultiplication()
    {
        foreach(Matrix<double> matrix in TestMatrices.List)
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
    public void MatrixMultiplication_DimensionCheck()
    {
        Assert.ThrowsException<ArgumentException>(() => TestMatrices._2x2_A * TestMatrices._3x2_B);
        Assert.ThrowsException<ArgumentException>(() => TestMatrices._3x2_A * TestMatrices._3x2_B);
    }
    [TestMethod]
    public void MatrixMultiplication_Correctness()
    {
        List<(Matrix<double> left, Matrix<double> right)> matrices = new()
        {
            (TestMatrices._2x2_A, TestMatrices._2x2_B),
            (TestMatrices._3x2_A, TestMatrices._2x3_B),
            (TestMatrices._2x3_A, TestMatrices._3x2_B)
        };
        foreach((Matrix<double> left, Matrix<double> right) in matrices)
        {
            Matrix<double> actual = left * right;
            Assert.AreEqual(actual.Dimensions, (left.RowCount, right.ColumnCount));
            foreach((int r, int c) in actual.Cells)
            {
                double expected = left.Row(r)
                                      .Zip(right.Column(c))
                                      .Select(x => x.First * x.Second)
                                      .Sum();
                Assert.AreEqual(expected, actual[r, c]);
            }
        }
    }
    [TestMethod]
    public void Inverse_Correctness()
    {
        // from https://en.wikipedia.org/wiki/Invertible_matrix#Examples
        Matrix<double> initial = new(new double[,]
        {
            { -1, 1.5 },
            { 1, -1 }
        });
        Matrix<double> expected = new(new double[,]
        {
            { 2, 3 },
            { 2, 2 }
        });
        Matrix<double> actual = initial.Inverse!;
        Assert.AreEqual(expected, actual);
    }
    [TestMethod]
    public void Identity()
    {
        Matrix<double> actual = Matrix<double>.Identity(2);
        Matrix<double> expected = new(new double[,]
        {
            { 1, 0 }, { 0, 1 }
        });
        Assert.AreEqual(expected, actual);
    }
}
