using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using d9.utl;

namespace citynames;
public class ArcGisQuery(string baseUrl, UrlQuery query)
{
    public static ArcGisQuery BiomeAt(LatLongPair coord) 
        => new("https://services.arcgis.com/P3ePLMYs2RVChkJx/arcgis/rest/services/Resolve_Ecoregions/FeatureServer/query",
                new([
                    ("layerDefs", "[{+\"layerId\"+:+\"0\",\"where\":+\"1=1\",+\"outfields\":+\"*\"}]"),
                    ("geometry", $"{{\"x\":+{coord.Longitude},+\"y\":+{coord.Latitude},+\"SpatialReference\":+{{\"wkid\":+4326}}}}"),
                    ("geometryType", "esriGeometryPoint"),
                    ("spatialRel", "esriSpatialRelIntersects"),
                    ("inSR", ""),
                    ("outSR", ""),
                    ("datumTransformation", ""),
                    ("applyVCSProjection", "false"),
                    ("returnGeometry", "false"),
                    ("maxAllowableOffset", ""),
                    ("geometryPrecision", ""),
                    ("returnIdsOnly", "false"),
                    ("returnCountOnly", "false"),
                    ("returnDistinctValues", "false"),
                    ("returnZ", "false"),
                    ("returnM", "false"),
                    ("sqlFormat", "none"),
                    ("f", "pjson"),
                    ("token", ""),
                    ("tolerance", "1000000")
        ]));
    public string Url => $"{baseUrl}{query}";
}
