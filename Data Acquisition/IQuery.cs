using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace citynames;
internal interface IQuery<T>
{
    internal static string BaseUrl { get; }
    internal Task<JsonDocument> GetAsync(T queryArg);
}
internal class ArcGisBiomeQuery : IQuery<LatLongPair>
{
    internal static string BaseUrl { get; private set; } = File.ReadAllText(@"C:\Users\dninemfive\Documents\workspaces\misc\citynames\arcgis query url.txt");
    internal async Task<JsonDocument> GetAsync(LatLongPair coords)
    {

    }
}