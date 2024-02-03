namespace citynames.tests;

[TestClass]
public class Tests_Querier
{
    [TestMethod]
    public async Task Test_GetBiomeAsync()
    {
        Assert.AreEqual("Mediterranean Forests, Woodlands & Scrub", (await Querier.GetBiomeAsync(34.3, -119)).result);
        Assert.AreEqual("Temperate Grasslands, Savannas & Shrublands", (await Querier.GetBiomeAsync(34.7, -101)).result);
    }
}