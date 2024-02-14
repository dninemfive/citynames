using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames;
public interface IStringGenerator
{
    public string RandomString { get; }
    public string RandomStringOfLength(int min, int max, int maxAttempts);
}
