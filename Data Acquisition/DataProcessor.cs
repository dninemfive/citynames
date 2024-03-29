namespace citynames;

/*
 * Input:
 * - query                  (character)
 * - context                (string or pair of characters)
 * Regression variables:
 * - biome                  (one-hot categorical)
 * - distance from coast    (float/stretch goal)
 * Output:
 * - probability that the queried character will occur
 */
public static class DataProcessor
{
    public static void WriteCsv(int contextLength = 2, bool writeToConsole = false)
    {
        Console.WriteLine(LogUtils.MethodArguments(arguments: [(nameof(contextLength), contextLength), (nameof(writeToConsole), writeToConsole)]));
        List<(string cityName, string biome)> allCityData = DataLoader.GetAllCityDataAsync()
                                                                   .ToBlockingEnumerable()
                                                                   .ToList();
        string fileName = $"transformedData.csv";
        File.WriteAllText(fileName, "");
        using FileStream fs = File.OpenWrite(fileName);
        using StreamWriter sw = new(fs);
        void write(string? s)
        {
            sw.WriteLine(s);
            if (writeToConsole)
                Console.WriteLine(s);
        }
        write(NgramInfo.CsvHeader);
        foreach (NgramInfo datum in allCityData.ToNgrams(contextLength))
            write(datum.CsvLine());
    }
}