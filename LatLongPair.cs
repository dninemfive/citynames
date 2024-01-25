namespace citynames;
public class LatLongPair
{
    /// <summary>
    /// Position wrt North-South, i.e. equivalent to "y".
    /// </summary>
    public double Latitude;
    /// <summary>
    /// Position wrt East-West, i.e. equivalent to "x".
    /// </summary>
    public double Longitude;
    public LatLongPair(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
    public void Deconstruct(out double latitude, out double longitude)
    {
        latitude = Latitude;
        longitude = Longitude;
    }
    public override string ToString()
        => $"({Latitude}°N {Longitude}°W)";
}
