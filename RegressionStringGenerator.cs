namespace citynames;
public class RegressionStringGenerator : IStringGenerator<RegressionQuery>
{
    public string RandomString(RegressionQuery query) => throw new NotImplementedException();

    public string RandomStringOfLength(RegressionQuery query, int min, int max, int maxAttempts)
        => throw new NotImplementedException();
}
public class RegressionQuery { }