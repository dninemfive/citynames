namespace citynames.tests;
[TestClass]
public class MatrixUtils
{
    [TestMethod]
    public void Dot()
    {
        Assert.AreEqual(15, new double[] { 1, 2, 3 }.Dot(new double[] { 4, -5, 7 }));
    }
    private class Array2DEqualityComparer : IEqualityComparer<double[,]>
    {
        public bool Equals(double[,]? a, double[,]? b)
        {
            if (a is null 
                || b is null 
                || a.GetLength(0) != b.GetLength(0) 
                || a.GetLength(1) != b.GetLength(1))
                return false;
            for (int x = 0; x < a.GetLength(0); x++)
                for (int y = 0; y < a.GetLength(1); y++)
                    if (a[x, y] != b[x, y])
                        return false;
            return true;
        }
        public int GetHashCode(double[,] array)
        {
            List<double> values = new();
            for (int x = 0; x < array.GetLength(0); x++)
                for (int y = 0; y < array.GetLength(1); y++)
                    values.Add(array[x, y]);
            return HashCode.Combine(values);
        }
    }
    [TestMethod]
    public void SwapRows()
    {
        Console.WriteLine("SwapRows()");
        double[,] initial = new double[,]
        {
            { 1, 2 },
            { 3, 4 },
            { 5, 6 }
        };
        double[,] expected = new double[,]
        {
            { 3, 4 },
            { 1, 2 },
            { 5, 6 }
        };
        double[,] actual = initial.SwapRows(0, 1);
        Console.WriteLine($"expected: {expected.MatrixString()}\nactual: {actual.MatrixString()}");
        Assert.AreEqual(expected, actual, new Array2DEqualityComparer(), $"expected: {expected.MatrixString()}\nactual: {actual.MatrixString()}");
    }
    [TestMethod]
    public void MultiplyRow()
    {
        Console.WriteLine("MultiplyRow()");
        double[,] initial = new double[,]
        {
            { 7, 9 },
            { 6, 4.5 }
        };
        double[,] expected = new double[,]
        {
            { 3.5, 4.5 },
            { 6, 4.5 }
        };
        double[,] actual = initial.MultiplyRow(0, 0.5);
        Console.WriteLine($"expected: {expected.MatrixString()}\nactual: {actual.MatrixString()}");
        Assert.AreEqual(expected, actual, new Array2DEqualityComparer(), $"expected: {expected.MatrixString()}\nactual: {actual.MatrixString()}");
    }
    [TestMethod]
    public void AddTwoRows()
    {
        Console.WriteLine("AddTwoRows()");
        double[,] initial = new double[,]
        {
            { 1, 2 },
            { 3, 4 },
            { 5, 6 }
        };
        double[,] expected = new double[,]
        {
            { 1, 2 },
            { 4, 6 },
            { 5, 6 }
        };
        double[,] actual = initial.AddTwoRows(1, 0, 1);
        Console.WriteLine($"expected: {expected.MatrixString()}\nactual: {actual.MatrixString()}");
        Assert.AreEqual(expected, actual, new Array2DEqualityComparer(), $"expected: {expected.MatrixString()}\nactual: {actual.MatrixString()}");
        expected = new double[,]
        {
            { 1, 2 },
            { 1.5, 1 },
            { 5, 6 }
        };
        actual = initial.AddTwoRows(1, 0, -1.5);
        Console.WriteLine($"expected: {expected.MatrixString()}\nactual: {actual.MatrixString()}");
        Assert.AreEqual(expected, actual, new Array2DEqualityComparer(), $"expected: {expected.MatrixString()}\nactual: {actual.MatrixString()}");
    }
}
