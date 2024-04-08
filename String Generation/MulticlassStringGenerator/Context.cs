using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames;
public class FixedLengthContext(int length, string str)
{
    public string[] Features => str.PadLeft(length, Characters.START)
                                   .Last(length)
                                   .Select(x => $"{x}")
                                   .ToArray();
    public static SchemaDefinition SchemaForLength(int length)
    {
        SchemaDefinition result = SchemaDefinition.Create();
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.ml.dataoperationscatalog.loadfromenumerable
        VectorDataViewType vectorType = new(((VectorDataViewType)result[0].ColumnType).ItemType, length);
        result[0].ColumnType = vectorType;
        return result;
    }
}