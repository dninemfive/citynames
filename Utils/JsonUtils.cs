using System.Text.Json;

namespace citynames;
public static class JsonUtils
{
    public static JsonElement? NullablyGetProperty(this JsonElement el, string propertyName)
        => el.TryGetProperty(propertyName, out JsonElement result) ? result : null;
    public static JsonElement? FirstArrayElement(this JsonElement el)
    {
        try
        {
            return el.EnumerateArray().First();
        }
        catch
        {
            return null;
        }
    }
}