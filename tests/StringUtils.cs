namespace citynames.tests;
[TestClass]
public class StringUtils
{
    const string _testString = "test string";
    [TestMethod]
    public void SubstringSafe_ZeroIndex()
    {
        Assert.AreEqual($"{_testString[0]}", _testString.SubstringSafe(0, 1));
        Assert.AreEqual($"{_testString[0]}", _testString.SubstringSafe(-1, 1));
    }
    [TestMethod]
    public void SubstringSafe()
    {
        for (int i = 1; i < _testString.Length; i++)
        {
            Assert.AreEqual($"{_testString[i]}", _testString.SubstringSafe(i, i + 1));
            Assert.AreEqual(_testString.Substring(i - 1, 2), _testString.SubstringSafe(i - 1, i + 1));
        }
    }
    [TestMethod]
    public void Last()
    {
        Assert.AreEqual(_testString[^2..], _testString.Last(2));
        Assert.AreEqual(_testString, _testString.Last(_testString.Length + 1));
    }
}
