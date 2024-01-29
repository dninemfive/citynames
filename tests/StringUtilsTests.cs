namespace citynames.tests;
[TestClass]
public class StringUtilsTests
{
    const string _testString = "test string";
    [TestMethod]
    public void SubstringSafeTest()
    {
        for(int i = 0; i < _testString.Length; i++)
        {
            Assert.AreEqual($"{_testString[i]}", _testString.SubstringSafe(i, i + 1));
            if(i == 0)
            {
                Assert.AreEqual($"{_testString[0]}", _testString.SubstringSafe(i - 1, i + 1));
            } 
            else
            {
                Assert.AreEqual(_testString.Substring(i - 1, 2), _testString.SubstringSafe(i - 1, i + 1));
            }
        }
    }
    [TestMethod]
    public void LastTest()
    {
        Assert.AreEqual(_testString[^2..], _testString.Last(2));
        Assert.AreEqual(_testString, _testString.Last(_testString.Length + 1));
    }
}
