using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames;
public class RegressionStringGenerator : IStringGenerator
{
    public string RandomString => throw new NotImplementedException();

    public string RandomStringOfLength(int min, int max, int maxAttempts)
        => throw new NotImplementedException();
}