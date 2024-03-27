namespace citynames;
/// <summary>
/// An attribute which marks the specified type as a potential option for string generation.
/// </summary>
/// <param name="name">The name used to select the type on the command line.</param>
/// <param name="fileName">The base name used to save and load the respective string generator.</param>
[AttributeUsage(AttributeTargets.Class)]
public class GeneratorAttribute(string name, string fileName) : Attribute
{
    public string Name { get; private set; } = name;
    public string BaseFileName { get; private set; } = fileName;

    public string FileNameFor(int contextLength)
        => BaseFileName.Replace("{contextLength}", $"{contextLength}");
}