using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace citynames;
public readonly ref struct Matrix<T>
    where T : INumberBase<T>
{
    private readonly T[,] _data;
    public int Rows => _data.GetLength(1);
    public int Columns => _data.GetLength(0);
    public (int columns, int rows) Dimensions => (Columns, Rows);
    public IEnumerable<(int x, int y)> Cells
        => MatrixUtils.AllCoordsFor(Columns, Rows);
    public T this[int x, int y] => _data[x, y];
    private static T[,] ArrayMatching(Matrix<T> m) => new T[m.Columns, m.Rows];
    private void ThrowIfOutOfBounds(int input, int max, string? paramName)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(input, paramName);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(input, max, paramName);
    }
    public T[] Column(int column)
    {
        ThrowIfOutOfBounds(column, Columns, nameof(column));
        T[] result = new T[Rows];
        for (int r = 0; r < Rows; r++)
            result[r] = this[column, r];
        return result;
    }
    public T[] Row(int row)
    {
        ThrowIfOutOfBounds(row, Rows, nameof(row));
        T[] result = new T[Columns];
        for (int c = 0; c < Columns; c++)
            result[c] = this[c, row];
        return result;
    }
    public Matrix(T[,] data)
    {
        _data = data;
    }
    public Matrix<T> Transpose
    {
        get
        {
            T[,] result = ArrayMatching(this);
            foreach((int i, int j) in Cells)
                result[j, i] = _data[i, j];
            return result;
        }
    }
    public static implicit operator Matrix<T>(T[,] data) 
        => new(data);
    public static implicit operator T[,](Matrix<T> m)
        => m._data;
    public Matrix<T> Apply(Func<T, T> func)
    {
        // would define in terms of Matrix.WithDimensions but that anonymous method thing
        T[,] result = ArrayMatching(this);
        foreach ((int x, int y) in Cells)
            result[x, y] = func(this[x, y]);
        return result;
    }
    public static Matrix<T> WithDimensions(int width, int height, Func<int, int, T> valueSetter)
    {
        T[,] result = new T[width, height];
        foreach ((int x, int y) in MatrixUtils.AllCoordsFor(width, height))
            result[x, y] = valueSetter(x, y);
        return result;
    }
    public static Matrix<T> operator+(Matrix<T> a, Matrix<T> b)
    {
        if (a.Dimensions != b.Dimensions)
            throw new ArgumentException($"Attempted to add matrices with dimensions {a.Columns}x{a.Rows} and {b.Columns}x{b.Rows}, but their dimensions must be the same!");
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
        if (left.Columns != right.Rows)
            throw new ArgumentException("Multiplication is only defined when the left matrix has exactly as many columns as the right has rows," +
            $"but the left has {left.Columns} and the right has {right.Rows}!");
        T[,] result = new T[right.Columns, left.Rows];
        foreach ((int c, int r) in MatrixUtils.AllCoordsFor(right.Columns, left.Rows))
            result[c, r] = left.Row(r).Dot(right.Column(c));
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
        foreach ((int x, int y) in MatrixUtils.AllCoordsFor(a.Columns, a.Rows))
            if (a[x, y] != b[x, y])
                return false;
        return true;
    }
    public static bool operator !=(Matrix<T> a, Matrix<T> b)
        => !(a == b);
    public override int GetHashCode()
        => _data.GetHashCode();
    public bool IsInvertible
        => Rows == Columns && this * Transpose == Identity(Rows);
    public bool TryInvert([MaybeNullWhen(false)]out Matrix<T> result)
    {
        if (!IsInvertible)
        {
            result = default;
            return false;
        }
        throw new NotImplementedException();
    }
}
public static class MatrixUtils
{
    public static IEnumerable<(int x, int y)> AllCoordsFor(int width, int height)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                yield return (x, y);
    }
    public static IEnumerable<(int x, int y)> AllCoords(this (int x, int y) dimensions)
        => AllCoordsFor(dimensions.x, dimensions.y);
    public static T Sum<T>(this IEnumerable<T> enumerable)
        where T : INumberBase<T>
        => enumerable.Aggregate((x, y) => x + y);
    public static T Dot<T>(this T[] a, T[] b)
        where T : INumberBase<T>
        => a.Zip(b)
            .Select(x => x.First * x.Second)
            .Sum();
}
