using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames;
public class BuildOrLoadInfo
{
    public bool Build => Path is null && (Ngrams ?? throw new Exception()) is not null;
    public string? Path { get; private set; } = null;
    public IEnumerable<NgramInfo>? Ngrams { get; private set; } = null;
    public int ContextLength { get; private set; }
    private BuildOrLoadInfo(int contextLength)
        => ContextLength = contextLength;
    public BuildOrLoadInfo(string path, int contextLength = 2) : this(contextLength)
        => Path = path;
    public BuildOrLoadInfo(IEnumerable<NgramInfo> ngrams, int contextLength = 2) : this(contextLength)
        => Ngrams = ngrams;
}
