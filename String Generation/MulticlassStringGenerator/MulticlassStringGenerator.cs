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
public class MulticlassStringGenerator : IStringGenerator<NgramInfo>, IAsyncSaveLoadable<MulticlassStringGenerator>
{
    private readonly MLContext _mlContext = new();
    public IDataView Data { get; private set; }
    public EstimatorChain<ColumnConcatenatingTransformer> Pipeline { get; private set; }
    public ITransformer Model { get; private set; }
    private PredictionEngine<NgramInfo, CharacterPrediction>? _predictionEngine;
    public PredictionEngine<NgramInfo, CharacterPrediction> PredictionEngine
    {
        get
        {
            _predictionEngine ??= _mlContext.Model.CreatePredictionEngine<NgramInfo, CharacterPrediction>(Model);
            return _predictionEngine;
        }
    }
    public readonly TextLoader.Options CsvLoaderOptions = new() { HasHeader = true, Separators = new char[] { ','} };
    public MulticlassStringGenerator(string path, TextLoader.Options? options = null)
    {
        Console.WriteLine($"Loading MulticlassStringGenerator from `{path}`...");
        Data = _mlContext.Data.LoadFromTextFile<NgramInfo>(path, options ?? CsvLoaderOptions);
        Pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label", "Successor")
                             .Append(_mlContext.Transforms.Categorical.OneHotEncoding("BiomeEncoded", "Biome"))
                             .Append(_mlContext.Transforms.Text.FeaturizeText("ContextEncoded", "Context"))
                             .Append(_mlContext.Transforms.Concatenate("Features", "BiomeEncoded", "ContextEncoded"));
        Model = Pipeline.Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                        .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"))
                        .Fit(Data);
        Console.WriteLine($"Loaded MulticlassStringGenerator from `{path}`.");
    }
    public async Task SaveAsync(string name = "model.zip")
        => await Task.Run(() => _mlContext.Model.Save(Model, Data.Schema, name));
    public static Task<MulticlassStringGenerator> LoadAsync(string path)
    {
        throw new NotImplementedException();
    }
    public CharacterPrediction Predict(NgramInfo input)
        => PredictionEngine.Predict(input);
    public char RandomChar(NgramInfo input)
    {
        // todo: find out how to map weights to their respective characters
        throw new NotImplementedException();
    }
    public string RandomString(NgramInfo input)
    {
        string context = "", result = "";
        while (true)
        {
            context = $"{context}{RandomChar(input)}".Last(2);
            if (context.Contains(Characters.STOP))
                break;
            result += context.Last();
        }
        return result.Replace($"{Characters.STOP}", "");
    }
}
