﻿namespace citynames;
/// <summary>
/// Basically a wrapper for a ( <see langword="double"/>, <see langword="double"/>) tuple which lets
/// me compare latitude and longitude more coarsely and prevents being mixed up between the
/// conflicting Wikidata and ArcGIS coordinate notation.
/// </summary>
public class LatLongPair(double latitude, double longitude) : IDictionaryable, IEquatable<LatLongPair>
{
    /// <summary>
    /// Position wrt North-South, i.e. equivalent to "y".
    /// </summary>
    /// <remarks>
    /// Rounded to the nearest hundredths place so it can be usefully used as a dictionary key.
    /// </remarks>
    public double Latitude { get; private set; } = latitude is >= -90 and <= 90 ? Math.Round(latitude, 2)
                                                                                : throw new ArgumentOutOfRangeException(nameof(latitude));
    /// <summary>
    /// Position wrt East-West, i.e. equivalent to "x".
    /// </summary>
    /// <remarks>
    /// Rounded to the nearest hundredths place so it can be usefully used as a dictionary key.
    /// </remarks>
    public double Longitude { get; private set; } = longitude is > -180 and <= 180 ? Math.Round(longitude, 2)
                                                                                   : throw new ArgumentOutOfRangeException(nameof(longitude));
    public void Deconstruct(out double latitude, out double longitude)
    {
        latitude = Latitude;
        longitude = Longitude;
    }
    private static string DegreeNotation(double d, char positive, char negative)
        => $"{Math.Abs(d):F2}°{(d < 0 ? negative : positive)}";
    public override string ToString()
        => $"({DegreeNotation(latitude, 'N', 'S'),7}, {DegreeNotation(longitude, 'E', 'W'),8})";
    public static bool operator ==(LatLongPair a, LatLongPair b)
        => a.Latitude == b.Latitude && a.Longitude == b.Longitude;
    public static bool operator !=(LatLongPair a, LatLongPair b)
        => !(a == b);
    public override bool Equals(object? obj)
        => obj is LatLongPair other && this == other;
    bool IEquatable<LatLongPair>.Equals(LatLongPair? other)
        => other is not null && this == other;
    public override int GetHashCode()
        => HashCode.Combine(Latitude, Longitude);
    public IReadOnlyDictionary<string, object?> ToDictionary()
        => new Dictionary<string, object?>()
        {
            { "x", Longitude },
            { "y", Latitude }
        };
    public string ToWkt(string name)
        => $"\"POINT ({Longitude} {Latitude})\",{name},";
}