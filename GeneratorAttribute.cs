using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames;
[AttributeUsage(AttributeTargets.Class)]
public class GeneratorAttribute : Attribute
{
    public string Name { get; private set; }
    public string BaseFileName { get; private set; }
    public GeneratorAttribute(string name, string fileName)
    {
        Name = name;
        BaseFileName = fileName;
    }
    public string FileNameFor(int contextLength)
        => BaseFileName.Replace("{contextLength}", $"{contextLength}");
}
