using d9.utl;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames;
public class MulticlassStringGenerator : IBuildLoadAbleStringGenerator<NgramInfo, MulticlassStringGenerator>
{
    private readonly MLContext _mlContext = new();
    private IDataView _data;
    public IDataView Data
    {
        get => _data;
        set
        {
            _data = value;
            Model = Pipeline.Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"))
                            .Fit(Data);
            // dirty the key-value maps in case they're sensitive to data used
            _keyValueMapper = null;
        }
    }
    private Dictionary<int, string>? _keyValueMapper = null;
    public IReadOnlyDictionary<int, string> KeyValueMapper
    {
        get
        {
            if(_keyValueMapper is null)
            {
                DataDebuggerPreview preview = Model.Preview(Data, maxRows: int.MaxValue);
                ImmutableArray<DataDebuggerPreview.ColumnInfo> columnView = preview.ColumnView;
                IEnumerable<int> ids = columnView.First(x => x.Column.Name == "Label").Values.Select(x => int.Parse($"{x}"));
                IEnumerable<string> characters = columnView.First(x => x.Column.Name == "Successor").Values.Select(x => $"{x}");
                // dictionary in case this ends up sparse somehow
                _keyValueMapper = new();
                foreach ((int key, string value) in ids.Zip(characters))
                    _keyValueMapper[key] = value;
            }
            return _keyValueMapper;
        }
    }
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
    public string RandomChar(NgramInfo input)
        => KeyValueMapper[Predict(input).CharacterWeights.Argrand() + 1];
    /*
    {
        float[] weights = Predict(input).CharacterWeights;
        // alternatively: threshold?
        IEnumerable<(int index, float weight)> top10 = 0.To(weights.Length)
                                                        .Zip(weights)
                                                        .OrderBy(x => x.Second)
                                                        .Take(10);
        return KeyValueMapper[top10.WeightedRandomElement(x => x.weight).index];
    }*/
    public string RandomString(NgramInfo input, int maxLength = 100)
    {
        string context = input.Context, result = input.Context;
        int ct = 0;
        while (++ct < maxLength)
        {
            context = $"{context}{RandomChar(input)}".Last(2);
            if (context.Contains(Characters.STOP))
                break;
            result += context.Last();
        }
        return result.Replace($"{Characters.STOP}", "");
    }
}
