using System.Text.Json.Serialization;

namespace citynames;
public class LatLongPair
{
    /// <summary>
    /// Position wrt North-South, i.e. equivalent to "y".
    /// </summary>
    public double Latitude { get; private set; }
    /// <summary>
    /// Position wrt East-West, i.e. equivalent to "x".
    /// </summary>
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
