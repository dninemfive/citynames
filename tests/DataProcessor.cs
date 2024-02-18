using d9.utl;

namespace citynames.tests;
[TestClass]
public class DataProcessor
{
    [TestMethod]
    public void DataFrom()
    {
        NgramInfo[] result = citynames.DataProcessor.DataFrom("test", "n/a", 2).ToArray();
        NgramInfo[] expected = new NgramInfo[]
        {
            new("", 't', ""),
            new("t", 'e', ""),
            new("te", 's', ""),
            new("es", 't', ""),
            new("st", citynames.DataProcessor.STOP, "")
        };
        Console.WriteLine($"result:   {result.ListNotation()}");
        Console.WriteLine($"expected: {expected.ListNotation()}");
        Assert.AreEqual(expected.Length, result.Length);
        for(int i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], result[i]);
    }
}
