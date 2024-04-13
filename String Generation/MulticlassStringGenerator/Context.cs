using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames;
public class MulticlassFeatures
{

    public float[] BiomeWeights;
    public string[] Ancestors;
    public float CoastDistance, Elevation;
    public MulticlassFeatures(IReadOnlyDictionary<string, float> biomeWeights,
                              string[] ancestors,
                              float CoastDistance,
                              float Elevation,
                              VectorEncoding<string, float> biomeEncoding)
    {

    }
}