using citynames;

internal class Program
{
    private static void Main(string[] args)
    {
        IEnumerable<string> cityNames = File.ReadAllLines("C:/Users/dninemfive/Downloads/geonames-all-cities-with-a-population-1000.csv")
                                            .Select(x => x.Split(";")[1]);
        MarkovStringGenerator markov = new(cityNames);
        Console.WriteLine(markov.MostCommonPairs(10));
        List<string> names = new();
        for (int i = 0; i < 10; i++)
            names.Add($"{markov.RandomStringOfLength(min: 5)}");
        File.WriteAllLines("city names.txt", names);
        File.WriteAllText("most common pairs.txt", markov.MostCommonPairs());
    }
}