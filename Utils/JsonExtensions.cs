using System.Text.Json;

namespace citynames;
/// <summary>
/// Some convenient extensions for <c>System.Text.Json</c> elements.
/// </summary>
public static class JsonExtensions
{
    /// <summary>
    /// Gets the first item of the specified <paramref name="element"/>, if possible.
    /// </summary>
    /// <param name="element">The element whose first item to get.</param>
    /// <returns>The first item, if the <paramref name="element"/> is an array and is not empty,
    ///          or <see langword="null"/> otherwise.</returns>
    public static JsonElement? FirstArrayElement(this JsonElement element)
    {
        try
        {
            return element.EnumerateArray().First();
        }
        catch
        {
            return null;
        }
    }
    /// <summary>
    /// Tries to get a property on the specified <paramref name="element"/> with the specified
    /// <paramref name="propertyName"/>, if any.
    /// </summary>
    /// <param name="element">The element whose property to get.</param>
    /// <param name="propertyName">The name of the property to get.</param>
    /// <returns>The property specified as above, or <see langword="null"/> if no such property is
    ///          found.</returns>
    public static JsonElement? NullablyGetProperty(this JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out JsonElement result) ? result : null;
}