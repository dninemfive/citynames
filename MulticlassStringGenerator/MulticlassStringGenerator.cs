using d9.utl;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames;
public class MulticlassStringGenerator : IStringGenerator<string>
{
    private readonly MLContext _mlContext = new();
    public IDataView Data { get; private set; }
    public EstimatorChain<ColumnConcatenatingTransformer> Pipeline { get; private set; }
    public ITransformer Model { get; private set; }
    private PredictionEngine<BigramFeature, CharacterPrediction>? _predictionEngine;
    public PredictionEngine<BigramFeature, CharacterPrediction> PredictionEngine
    {
        get
        {
            _predictionEngine ??= _mlContext.Model.CreatePredictionEngine<BigramFeature, CharacterPrediction>(Model);
            return _predictionEngine;
        }
    }
    public readonly TextLoader.Options CsvLoaderOptions = new() { HasHeader = true, Separators = new char[] { ','} };
    public MulticlassStringGenerator(string path, TextLoader.Options? options = null)
    {
        Console.WriteLine($"Loading MulticlassStringGenerator from `{path}`...");
        Data = _mlContext.Data.LoadFromTextFile<BigramFeature>(path, options ?? CsvLoaderOptions);
        Pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label", "Successor")
                             .Append(_mlContext.Transforms.Categorical.OneHotEncoding("BiomeEncoded", "Biome"))
                             .Append(_mlContext.Transforms.Text.FeaturizeText("ContextEncoded", "Context"))
                             .Append(_mlContext.Transforms.Concatenate("Features", "BiomeEncoded", "ContextEncoded"));
        Model = Pipeline.Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                        .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"))
                        .Fit(Data);
        Console.WriteLine($"Loaded MulticlassStringGenerator from `{path}`.");
    }
    public void Save(string name = "model.zip")
        => _mlContext.Model.Save(Model, Data.Schema, name);
    public CharacterPrediction Predict(BigramFeature input)
        => PredictionEngine.Predict(input);
    public char RandomChar(string context, string biome)
    {
        BigramFeature input = new(context, Constants.NullCharacter, biome);
        // todo: find out how to map weights to their respective characters
        throw new NotImplementedException();
    }
    public string RandomString(string biome)
    {
        string context = "", result = "";
        while (true)
        {
            context = $"{context}{RandomChar(context, biome)}".Last(2);
            if (context.Contains(DataProcessor.STOP))
                break;
            result += context.Last();
        }
        return result.Replace($"{DataProcessor.STOP}", "");
    }
    public string RandomStringOfLength(string input, int min = 1, int max = int.MaxValue, int maxAttempts = 100)
    {
        throw new NotImplementedException();
    }
}
