using d9.utl;

namespace citynames.tests;
[TestClass]
public class DataProcessor
{
    [TestMethod]
    public void DataFrom()
    {
        NgramInfo[] result = "test".NgramInfos("n/a", 2).ToArray();
        NgramInfo[] expected = new NgramInfo[]
        {
            new("", "t", ""),
            new("t", "t", ""),
            new("te", "t", ""),
            new("es", "t", ""),
            new("st", $"{Characters.STOP}", "")
        };
        Console.WriteLine($"result:   {result.ListNotation()}");
        Console.WriteLine($"expected: {expected.ListNotation()}");
        Assert.AreEqual(expected.Length, result.Length);
        for(int i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], result[i]);
    }
}
