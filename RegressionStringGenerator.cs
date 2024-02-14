using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames;
public class RegressionStringGenerator : IStringGenerator
{
}
public class ContextCharDataItem
{
    public string Context;
    public char Result;
    public OneHotCategoryEncoding Biome;
    public float DistanceFromCoast;
}
public class OneHotCategoryEncoding
{

}