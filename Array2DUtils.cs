using System.Numerics;

namespace citynames;

public static class Array2DUtils
{
    #region elementary matrix operations
    public static T[,] AddTwoRows<T>(this T[,] array, int targetRow, int sourceRow, T scalar)
        where T : INumberBase<T>, IComparisonOperators<T, T, bool>
    {
        Console.WriteLine($"AddTwoRows({array.MatrixString()}, {targetRow}, {sourceRow}, {scalar})");
        T[,] result = array.Copy();
        for (int c = 0; c < result.GetLength(1); c++)
            result[targetRow, c] += array[sourceRow, c] * scalar;
        Console.WriteLine(result.MatrixString());
        return result;
    }
    public static T[,] MultiplyRow<T>(this T[,] array, int row, T scalar)
        where T : INumberBase<T>, IComparisonOperators<T, T, bool>
    {
        Console.WriteLine($"MultiplyRow({array.MatrixString()}, {row}, {scalar})");
        T[,] result = array.Copy();
        for (int c = 0; c < result.GetLength(1); c++)
            result[row, c] = scalar * array[row, c];
        Console.WriteLine(result.MatrixString());
        return result;
    }
    public static T[,] SwapRows<T>(this T[,] array, int rowA, int rowB)
        where T : INumberBase<T>, IComparisonOperators<T, T, bool>
    {
        Console.WriteLine($"SwapRows({array.MatrixString()}, {rowA}, {rowB})");
        T[,] result = array.Copy();
        for (int c = 0; c < result.GetLength(1); c++)
        {
            result[rowA, c] = array[rowB, c];
            result[rowB, c] = array[rowA, c];
        }
        Console.WriteLine(result.MatrixString());
        return result;
    }
    #endregion elementary matrix operations
    public static IEnumerable<(int row, int columns)> AllCoordsFor(int rows, int columns)
        => 0.To(rows).CrossJoin(0.To(columns));
    public static IEnumerable<(int row, int column)> AllCoordsFor((int rows, int columns) dimensions)
        => AllCoordsFor(dimensions.rows, dimensions.columns);
    public static T[,] Copy<T>(this T[,] array)
    {
        T[,] result = new T[array.GetLength(0), array.GetLength(1)];
        foreach ((int i, int j) in AllCoordsFor(array.GetLength(0), array.GetLength(1)))
            result[i, j] = array[i, j];
        return result;
    }
    public static T[,] MatchingArray<T>(this Matrix<T> m)
        where T : INumberBase<T>, IComparisonOperators<T, T, bool>
        => new T[m.RowCount, m.ColumnCount];
    public static string MatrixString<T>(this T[,] m)
        where T : INumberBase<T>, IComparisonOperators<T, T, bool>
        => $"{new Matrix<T>(m)}";
    public static T[,] Transpose<T>(this T[,] array)
    {
        T[,] result = new T[array.GetLength(1), array.GetLength(0)];
        foreach ((int i, int j) in AllCoordsFor(array.GetLength(0), array.GetLength(1)))
            result[j, i] = array[i, j];
        return result;
    }
}