using System.Text.Json.Serialization;

namespace citynames;
/// <summary>
/// Basically a wrapper for a (<see langword="double"/>, <see langword="double"/>) tuple
/// which lets me compare latitude and longitude more coarsely and prevents being mixed up
/// between the conflicting Wikidata and ArcGIS coordinate notation.
/// </summary>
public class LatLongPair
{
    /// <summary>
    /// Position wrt North-South, i.e. equivalent to "y".
    /// </summary>
    /// <remarks>Rounded to the nearest hundredths place so it can be usefully used as a dictionary key.</remarks>
    public double Latitude { get; private set; }
    /// <summary>
    /// Position wrt East-West, i.e. equivalent to "x".
    /// </summary>
    /// <remarks>Rounded to the nearest hundredths place so it can be usefully used as a dictionary key.</remarks>
    public double Longitude { get; private set; }
    public LatLongPair(double latitude, double longitude)
    {
        Latitude = Math.Round(latitude, 2);
        Longitude = Math.Round(longitude, 2);
    }
    public void Deconstruct(out double latitude, out double longitude)
    {
        latitude = Latitude;
        longitude = Longitude;
    }
    public override string ToString()
        => $"({Latitude}°N {Longitude}°E)";
    [JsonIgnore]
    public string TableString
        => $"{Latitude,6:F2}°N {Longitude,6:F2}°E";
    public override bool Equals(object? obj)
        => obj is LatLongPair other && Latitude == other.Latitude && Longitude == other.Longitude;
    public override int GetHashCode()
        => HashCode.Combine(Latitude, Longitude);
}
