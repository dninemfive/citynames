namespace citynames.tests;
[TestClass]
public class StringUtilsTests
{
    [TestMethod]
    public void SubstringSafeTest()
    {
        string testString = "test string";
        for(int i = 0; i < testString.Length; i++)
        {
            Assert.AreEqual($"{testString[i]}", testString.SubstringSafe(i, i + 1));
            if(i == 0)
            {
                Assert.AreEqual($"{testString[0]}", testString.SubstringSafe(i - 1, i + 1));
            } 
            else
            {
                Assert.AreEqual(testString.Substring(i - 1, 2), testString.SubstringSafe(i - 1, i + 1));
            }
        }
    }
}
