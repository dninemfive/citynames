namespace citynames.tests;

[TestClass]
public class QuerierTests
{
    [TestMethod]
    public async Task GetBiomeAsyncTest()
    {
        Assert.AreEqual("Mediterranean Forests, Woodlands & Scrub", (await Querier.GetBiomeAsync(34.3, -119)).result);
        Assert.AreEqual("Temperate Grasslands, Savannas & Shrublands", (await Querier.GetBiomeAsync(34.7, -101)).result);
    }
}