namespace citynames.tests;

[TestClass]
public class Querier
{
    [TestMethod]
    public async Task GetBiomesAsync()
    {
        Assert.AreEqual("Mediterranean Forests, Woodlands & Scrub", (await citynames.DataLoader.GetBiomeAsync(34.3, -119)).result);
        Assert.AreEqual("Temperate Grasslands, Savannas & Shrublands", (await citynames.DataLoader.GetBiomeAsync(34.7, -101)).result);
    }
}