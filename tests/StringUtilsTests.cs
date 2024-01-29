namespace citynames.tests;
[TestClass]
public class StringUtilsTests
{
    [TestMethod]
    public void SubstringSafeTest()
    {
        string testString = "test string";
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => testString.SubstringSafe(0, 3));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => testString.SubstringSafe(1, -1));
        for(int i = 0; i < testString.Length; i++)
        {
            Assert.AreEqual($"{testString[i]}", testString.SubstringSafe(1, i));
            if(i == 0)
            {
                Assert.AreEqual($"{testString[0]}", testString.SubstringSafe(2, 0));
            } 
            else
            {
                Assert.AreEqual(testString.Substring(i - 1, 2), testString.SubstringSafe(2, i));
            }
        }
    }
}
