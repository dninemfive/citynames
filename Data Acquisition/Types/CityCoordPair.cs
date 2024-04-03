using System.Diagnostics.CodeAnalysis;

namespace citynames;
/// <summary>
/// Convenience type for deduplicating cities by checking both name and coordinates.
/// </summary>
/// <param name="name">The name of the city.</param>
/// <param name="coordinates">The geospacial coordinates of the city.</param>
public readonly struct CityCoordPair(string name, LatLongPair coordinates)
    : IEquatable<CityCoordPair>
{
    public readonly string Name = name;
    public readonly LatLongPair Coordinates = coordinates;
    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is CityCoordPair pair && Name == pair.Name && Coordinates == pair.Coordinates;
    public override int GetHashCode()
        => HashCode.Combine(Name, Coordinates);
    public override string ToString()
        => $"({Name}, {Coordinates})";
    public static bool operator ==(CityCoordPair left, CityCoordPair right)
        => left.Equals(right);
    public static bool operator !=(CityCoordPair left, CityCoordPair right)
        => !(left == right);
    public void Deconstruct(out string name, out LatLongPair coordinates)
    {
        name = Name;
        coordinates = Coordinates;
    }
    public bool Equals(CityCoordPair other)
        => this == other;
    public static implicit operator (string name, LatLongPair coords)(CityCoordPair pair)
        => (pair.Name, pair.Coordinates);
}