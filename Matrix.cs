using d9.utl;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Linq;

namespace citynames;
public class Matrix<T>
    where T : INumberBase<T>, IComparisonOperators<T, T, bool>
{
    #region fields
    private readonly T[,] _data;
    #endregion fields
    #region trivial properties
    public int RowCount => _data.GetLength(0);
    public int ColumnCount => _data.GetLength(1);
    public (int RowCount, int ColumnCount) Dimensions 
        => (RowCount, ColumnCount);
    public string DimensionString
        => $"{RowCount}×{ColumnCount}";
    public IEnumerable<(int row, int column)> Cells
        => Array2DUtils.AllCoordsFor(Dimensions);
    #endregion trivial properties
    #region static helper methods
    public static Matrix<T> Identity(int n)
    {
        T[,] result = new T[n, n];
        foreach ((int x, int y) in Array2DUtils.AllCoordsFor(n, n))
            result[x, y] = x == y ? T.MultiplicativeIdentity : T.Zero;
        return result;
    }
    private static ArgumentException MismatchException(Matrix<T> a, Matrix<T> b, string op, string explanation = "their dimensions must be the same")
        => new($"Attempted to {op} {a.DimensionString} and {b.DimensionString} matrices, but {explanation}!");
    private static void ThrowIfOutOfBounds(int input, int max, string? paramName)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(input, paramName);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(input, max, paramName);
    }
    public static Matrix<T> WithDimensions(int rows, int columns, Func<int, int, T> valueSetter)
    {
        T[,] result = new T[rows, columns];
        foreach ((int x, int y) in Array2DUtils.AllCoordsFor(rows, columns))
            result[x, y] = valueSetter(x, y);
        return result;
    }
    #endregion static helper methods
    #region indexers
    public T this[int row, int column] => _data[row, column];
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
    #endregion indexers
    public Matrix(T[,] data)
    {
        _data = data;
    }
    #region operators
    public static implicit operator Matrix<T>(T[,] data)
        => new(data);
    public static implicit operator T[,](Matrix<T> m)
        => m._data;
    public static Matrix<T> operator +(Matrix<T> a, Matrix<T> b)
    {
        if (a.Dimensions != b.Dimensions)
            throw MismatchException(a, b, "add");
        T[,] result = a.MatchingArray();
        foreach ((int x, int y) in a.Cells)
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
        if (left.ColumnCount != right.RowCount)
            throw MismatchException(left, right, "multiply", "the left matrix must have exactly as many columns as the right has rows");
        T[,] result = new T[left.RowCount, right.ColumnCount];
        foreach ((int r, int c) in Array2DUtils.AllCoordsFor(left.RowCount, right.ColumnCount))
            result[r, c] = left.Row(r).Dot(right.Column(c));
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
    #endregion operators
    #region nontrivial properties
    public Matrix<T> Augmented
        => HStack(Identity(RowCount));
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
    public Matrix<T> RowEchelonForm
    {
        get
        {
            T[,] result = _data.Copy();
            throw new NotImplementedException();
            // https://www.math.purdue.edu/~shao92/documents/Algorithm%20REF.pdf
            // 1. (given)
            // 2. determine leftmost nonzero column
            // 3. use elementary row operations to put a 1 in the topmost (pivot) position of this column
            // 4. use EROs to put zeros strictly below the pivot position
            // 5. if there are no more non-zero rows below the pivot position, return
            // 6. apply 2-5 to the rows below the current
        }
    }
    public Matrix<T> ReducedRowEchelonForm
    { 
        get
        {
            T[,] result = RowEchelonForm;
            throw new NotImplementedException();
            // https://www.math.purdue.edu/~shao92/documents/Algorithm%20REF.pdf
            // 8. find all the leading rows
            // 9. determine the rightmost column containing a leading one
            // 10. use type III elementary row operations (multiplications?) to erase nonzero entries above the leading one
            // 11. if there are no more columns containing leading ones, return
            // 12. apply 9-11 to the columns left of the pivot
        }
    }
    // https://en.wikipedia.org/wiki/Gaussian_elimination#Pseudocode
    public Matrix<T> Rref
    {
        get
        {
            T[,] result = _data.Copy();
            int h = 0, k = 0;
            while (h < RowCount && k < ColumnCount)
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
    public Matrix<T> Transposition
        => _data.Transpose();
    #endregion
    #region overrides
    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is Matrix<T> other && this == other;
    public override int GetHashCode()
        => _data.GetHashCode();
    public override string ToString()
    {
        int resultLength = RowCount + 2;
        string[] result = new string[resultLength];
        for (int r = 1; r < resultLength - 1; r++)
            result[r] = "│ ";
        for (int c = 0; c < ColumnCount; c++)
        {
            IEnumerable<string> column = Column(c).Select(x => x.PrintNull("??"));
            int columnWidth = column.Select(x => x.Length).Max();
            for (int r = 0; r < column.Count(); r++)
                result[r + 1] += column.ElementAt(r).PadLeft(columnWidth) + "  ";
        }
        for (int r = 1; r < resultLength - 1; r++)
            result[r] = result[r].Trim() + " │";
        result[0] = "┌".PadRight(result[1].Length - 1) + "┐";
        result[^1] = "└".PadRight(result[^2].Length - 1) + "┘";
        return result.Aggregate((x, y) => $"{x}\n{y}");
    }
    #endregion overrides
    #region methods
    public Matrix<T> Apply(Func<T, T> func)
    {
        T[,] result = this.MatchingArray();
        foreach ((int row, int column) in Cells)
            result[row, column] = func(this[row, column]);
        return result;
    }
    public Matrix<T> HStack(Matrix<T> other)
    {
        if (RowCount != other.RowCount)
            throw new ArgumentException("Cannot stack two matrices with different row counts horizontally!");
        int newColumnCount = ColumnCount + other.ColumnCount;
        T[,] result = new T[RowCount, newColumnCount];
        for (int r = 0; r < RowCount; r++)
            for (int c = 0; c < newColumnCount; c++)
                result[r, c] = c < ColumnCount ? this[r, c] : other[r, c - ColumnCount];
        return result;
    }
    #endregion methods
    public bool IsInvertible
    //    => RowCount == ColumnCount && (this * Transposition) == Identity(RowCount);
        => true; //todo: determinant == 0 or something
}