using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using citynames;
using d9.utl;

namespace citynames.tests;
[TestClass]
public class DataProcessor
{
    [TestMethod]
    public void DataFrom()
    {
        Datum[] result = citynames.DataProcessor.DataFrom("test", "n/a", 2).ToArray();
        Datum[] expected = new Datum[]
        {
            new("n/a", "", 't'),
            new("n/a", "t", 'e'),
            new("n/a", "te", 's'),
            new("n/a", "es", 't'),
            new("n/a", "st", citynames.DataProcessor.STOP)
        };
        Console.WriteLine($"result:   {result.ListNotation()}");
        Console.WriteLine($"expected: {expected.ListNotation()}");
        Assert.AreEqual(expected.Length, result.Length);
        for(int i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], result[i]);
    }
}
