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
public class MulticlassStringGenerator : IBuildableStringGenerator<NgramInfo, MulticlassStringGenerator>
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
    public static readonly TextLoader.Options CsvLoaderOptions = new() { HasHeader = true, Separators = new char[] { ','} };
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value [...]: only called by LoadAsync and BuildAsync,
                               // which definitely initialize Data
    private MulticlassStringGenerator()
#pragma warning restore CS8618
    {
        Pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label", "Successor")
                             .Append(_mlContext.Transforms.Categorical.OneHotEncoding("BiomeEncoded", "Biome"))
                             .Append(_mlContext.Transforms.Text.FeaturizeText("ContextEncoded", "Context"))
                             .Append(_mlContext.Transforms.Concatenate("Features", "BiomeEncoded", "ContextEncoded"));
        Model = Pipeline.Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                        .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"))
                        .Fit(Data);
    }
    public async Task SaveAsync(string name = "model.zip")
        => await Task.Run(() => _mlContext.Model.Save(Model, Data.Schema, name));
    public static async Task<MulticlassStringGenerator> LoadAsync(string path)
    {
        MulticlassStringGenerator result = new();
        result.Data = await Task.Run(() => result._mlContext.Data.LoadFromTextFile<NgramInfo>(path, CsvLoaderOptions));
        return result;
    }
    public static async Task<MulticlassStringGenerator> BuildAsync(IAsyncEnumerable<NgramInfo> ngrams, int _ = 2)
    {
        MulticlassStringGenerator result = new();
        result.Data = await Task.Run(() => result._mlContext.Data.LoadFromEnumerable(ngrams.ToBlockingEnumerable()));
        return result;
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
