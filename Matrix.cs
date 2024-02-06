using d9.utl;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Linq;

namespace citynames;
public class Matrix<T>
    where T : INumberBase<T>, IComparisonOperators<T, T, bool>
{
    private readonly T[,] _data;
    public int RowCount => _data.GetLength(0);
    public int ColumnCount => _data.GetLength(1);
    public (int RowCount, int ColumnCount) Dimensions => (RowCount, ColumnCount);
    public IEnumerable<(int row, int column)> Cells
        => Dimensions.AllCoords();
    public T this[int row, int column] => _data[row, column];
    private static T[,] ArrayMatching(Matrix<T> m) => new T[m.RowCount, m.ColumnCount];
    private static void ThrowIfOutOfBounds(int input, int max, string? paramName)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(input, paramName);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(input, max, paramName);
    }
    public T[] Row(int row)
    {
        ThrowIfOutOfBounds(row, RowCount, nameof(row));
        T[] result = new T[ColumnCount];
        for (int c = 0; c < ColumnCount; c++)
            result[c] = this[row, c];
        return result;
    }
    public T[] Column(int column)
    {
        ThrowIfOutOfBounds(column, ColumnCount, nameof(column));
        T[] result = new T[RowCount];
        for (int r = 0; r < RowCount; r++)
            result[r] = this[r, column];
        return result;
    }
    public T[,] Columns(params int[] columns)
    {
        Console.WriteLine($"Columns({columns.ListNotation()})");
        T[,] result = new T[RowCount, columns.Length];
        for(int i = 0; i < columns.Length; i++)
            for (int r = 0; r < RowCount; r++)
                result[r, i] = this[r, columns[i]];
        return result;
    }
    public T[,] ColumnSlice(int start = 0, int end = int.MaxValue)
    {
        start = Math.Max(start, 0);
        end = Math.Min(end, ColumnCount);
        if (start > end)
            throw new ArgumentException($"Can't slice starting at a larger value ({start}) to a smaller one ({end})!");
        return Columns(start.To(end).ToArray());
    }
    public Matrix(T[,] data)
    {
        _data = data;
    }
    public Matrix<T> Transposition
        => _data.Transpose();
    public static implicit operator Matrix<T>(T[,] data) 
        => new(data);
    public static implicit operator T[,](Matrix<T> m)
        => m._data;
    public Matrix<T> Apply(Func<T, T> func)
    {
        // would define in terms of Matrix.WithDimensions but that anonymous method thing
        T[,] result = ArrayMatching(this);
        foreach ((int row, int column) in Cells)
            result[row, column] = func(this[row, column]);
        return result;
    }
    public static Matrix<T> WithDimensions(int rows, int columns, Func<int, int, T> valueSetter)
    {
        T[,] result = new T[rows, columns];
        foreach ((int x, int y) in MatrixUtils.AllCoordsFor(rows, columns))
            result[x, y] = valueSetter(x, y);
        return result;
    }
    public static Matrix<T> operator+(Matrix<T> a, Matrix<T> b)
    {
        if (a.Dimensions != b.Dimensions)
            throw new ArgumentException($"Attempted to add matrices with dimensions {a.ColumnCount}x{a.RowCount} " +
                $"and {b.ColumnCount}x{b.RowCount}, but their dimensions must be the same!");
        T[,] result = ArrayMatching(a);
        foreach((int x, int y) in a.Cells)
            result[x, y] = a[x, y] + b[x, y];
        return result;
    }
    public static Matrix<T> operator -(Matrix<T> m)
        => m.Apply(x => -x);
    public static Matrix<T> operator -(Matrix<T> a, Matrix<T> b)
        => a + -b;
    public static Matrix<T> operator *(T c, Matrix<T> m)
        => m.Apply(x => c * x);
    public static Matrix<T> operator *(Matrix<T> m, T c) 
        => c * m;
    public static Matrix<T> operator *(Matrix<T> left, Matrix<T> right)
    {
        Console.WriteLine($"Left matrix:\n{left}\nRight matrix:\n{right}");
        if (left.ColumnCount != right.RowCount)
            throw new ArgumentException("Multiplication is only defined when the left matrix has exactly as many columns as the right has rows," +
            $"but the left has {left.ColumnCount} and the right has {right.RowCount}!");
        T[,] result = new T[left.RowCount, right.ColumnCount];
        foreach ((int r, int c) in MatrixUtils.AllCoordsFor(left.RowCount, right.ColumnCount))
            result[r, c] = left.Row(r).Dot(right.Column(c));
        Console.WriteLine($"Result:\n{result.ToMatrix()}");
        return result;
    }
    public static Matrix<T> Identity(int n)
    {
        T[,] result = new T[n, n];
        foreach ((int x, int y) in MatrixUtils.AllCoordsFor(n, n))
            result[x, y] = x == y ? T.MultiplicativeIdentity : T.Zero;
        return result;
    }
    public static bool operator ==(Matrix<T> a, Matrix<T> b)
    {
        if (a.Dimensions != b.Dimensions)
            return false;
        foreach ((int row, int column) in a.Cells)
            if (a[row, column] != b[row, column])
                return false;
        return true;
    }
    public static bool operator !=(Matrix<T> a, Matrix<T> b)
        => !(a == b);
    public override int GetHashCode()
        => _data.GetHashCode();
    public bool IsInvertible
    //    => RowCount == ColumnCount && (this * Transposition) == Identity(RowCount);
        => true; //todo: determinant == 0 or something
    public Matrix<T>? Inverse
    {
        get
        {
            if (!IsInvertible)
                return null;
            Matrix<T> augmentedRref = Augmented.Rref;
            Console.WriteLine($"Augmented:\n{Augmented}\nAugmented RREF:\n{augmentedRref}");
            return augmentedRref.ColumnSlice(ColumnCount);
        }
    }
    public Matrix<T> SwapRows(int rowA, int rowB)
    {
        Console.WriteLine($"SwapRows({rowA}, {rowB})({rowA != rowB})");
        if (rowA == rowB) return this;
        Console.WriteLine($"Original:\n{this}");
        T[,] result = ArrayMatching(this);
        foreach((int row, int column) in Cells)
        {
            result[row, column] = (row == rowA, row == rowB) switch
            {
                (true, _) => this[rowB, column],
                (_, true) => this[rowA, column],
                _ => this[row, column]
            };
        }
        Console.WriteLine($"Result:\n{new Matrix<T>(result)}");
        return result;
    }
    public override string ToString()
    {
        int resultLength = RowCount + 2;
        string[] result = new string[resultLength];
        for (int r = 1; r < resultLength - 1; r++)
            result[r] = "│ ";
        for(int c = 0; c < ColumnCount; c++)
        {
            IEnumerable<string> column = Column(c).Select(x => x.PrintNull("??"));
            int columnWidth = column.Select(x => x.Length).Max();
            for(int r = 0; r < column.Count(); r++)
                result[r + 1] += column.ElementAt(r).PadLeft(columnWidth) + "  ";
        }
        for (int r = 1; r < resultLength - 1; r++)
            result[r] = result[r].Trim() + " │";
        result[0] = "┌".PadRight(result[1].Length - 1) + "┐";
        result[^1] = "└".PadRight(result[^2].Length - 1) + "┘";
        return result.Aggregate((x, y) => $"{x}\n{y}");
    }
    // https://en.wikipedia.org/wiki/Gaussian_elimination#Pseudocode
    public Matrix<T> Rref
    {
        get
        {
            T[,] result = this;
            int h = 0, k = 0;
            while(h < RowCount && k < ColumnCount)
            {
                int pivot = Column(k).Select(T.Abs)
                                     .Argmax();
                Console.WriteLine($"Rref(h = {h}, k = {k}, pivot = {pivot}):\n{new Matrix<T>(result)}");
                if (T.IsZero(this[pivot, k]))
                {
                    Console.WriteLine("\tPivot is 0");
                    k++;
                } 
                else
                {
                    result = result.SwapRows(h, pivot);
                    for (int i = h + 1; i < RowCount; i++)
                    {
                        Console.WriteLine($"i, k = {i}, {k}");
                        T f = result[i, k] / result[h, k];
                        Console.WriteLine($"\tf = {f}");
                        Console.WriteLine($"{result[i, k]}");
                        result = result.AddTwoRows(i, h, -f);
                        result[i, k] = T.Zero;
                    }
                    h++;
                    k++;
                }
            }
            return result;
        }
    }
    public Matrix<T> HStack(Matrix<T> other)
    {
        if (RowCount != other.RowCount)
            throw new ArgumentException("Cannot stack two matrices with different row counts horizontally!");
        int newColumnCount = ColumnCount + other.ColumnCount;
        T[,] result = new T[RowCount, newColumnCount];
        for(int r = 0; r < RowCount; r++)
            for (int c = 0; c < newColumnCount; c++)
                result[r, c] = c < ColumnCount ? this[r, c] : other[r, c - ColumnCount];
        return result;
    }
    public Matrix<T> Augmented
        => HStack(Identity(RowCount));
    public override bool Equals([NotNullWhen(true)]object? obj)
        => obj is Matrix<T> other && this == other;
}
public static class MatrixUtils
{
    public static IEnumerable<(int row, int columns)> AllCoordsFor(int rows, int columns)
        => ZeroTo(rows).CrossJoin(ZeroTo(columns));
    public static IEnumerable<(int row, int column)> AllCoords(this (int rows, int columns) dimensions)
        => AllCoordsFor(dimensions.rows, dimensions.columns);
    public static T Sum<T>(this IEnumerable<T> enumerable)
        where T : INumberBase<T>
        => enumerable.Aggregate((x, y) => x + y);
    public static T Dot<T>(this T[] a, T[] b)
        where T : INumberBase<T>
        => a.Zip(b)
            .Select(x => x.First * x.Second)
            .Sum();
    public static IEnumerable<int> To(this int a, int b)
    {
        for (int i = a; i < b; i++)
            yield return i;
    }
    public static IEnumerable<int> ZeroTo(int max)
        => 0.To(max);
    public static IEnumerable<(T, U)> CrossJoin<T, U>(this IEnumerable<T> ts, IEnumerable<U> us)
        => ts.SelectMany(t => us.Select(u => (t, u)));
    public static int Argmax<T>(this IEnumerable<T> items)
        where T : IComparisonOperators<T, T, bool>
    {
        T max = items.First();
        int result = 0;
        for (int i = 1; i < items.Count(); i++)
            if (items.ElementAt(i) > max)
                (max, result) = (items.ElementAt(i), i);
        return result;
    }
    public static T[,] Transpose<T>(this T[,] array)
    {
        T[,] result = new T[array.GetLength(1), array.GetLength(0)];
        foreach ((int i, int j) in AllCoordsFor(array.GetLength(0), array.GetLength(1)))
            result[j, i] = array[i, j];
        return result;
    }
    public static T[,] Copy<T>(this T[,] array)
    {
        T[,] result = new T[array.GetLength(0), array.GetLength(1)];
        foreach ((int i, int j) in AllCoordsFor(array.GetLength(0), array.GetLength(1)))
            result[i, j] = array[i, j];
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
    public static Matrix<T> ToMatrix<T>(this T[,] array)
        where T : INumberBase<T>, IComparisonOperators<T, T, bool>
        => array;
    public static string MatrixString<T>(this T[,] m)
        where T : INumberBase<T>, IComparisonOperators<T, T, bool>
        => $"{new Matrix<T>(m)}";

}
