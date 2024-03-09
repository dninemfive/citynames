using System.Text.Json;

namespace citynames;
internal static class JsonUtils
{
    internal static JsonElement? NullablyGetProperty(this JsonElement el, string propertyName)
        => el.TryGetProperty(propertyName, out JsonElement result) ? result : null;
    internal static JsonElement? FirstArrayElement(this JsonElement el)
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