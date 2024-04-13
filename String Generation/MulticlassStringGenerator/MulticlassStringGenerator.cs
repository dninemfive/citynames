using d9.utl;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Collections.Immutable;
using System.Data;

namespace citynames;
[Generator("multiclass", "multiclass.zip")]
public class MulticlassStringGenerator : IBuildLoadableStringGenerator<CityInfo, MulticlassStringGenerator>
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
            if (_keyValueMapper is null)
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
    private PredictionEngine<MulticlassFeatures, CharacterPrediction>? _predictionEngine;
    public PredictionEngine<MulticlassFeatures, CharacterPrediction> PredictionEngine
    {
        get
        {
            _predictionEngine ??= _mlContext.Model.CreatePredictionEngine<MulticlassFeatures, CharacterPrediction>(Model);
            return _predictionEngine;
        }
    }
    public VectorEncoding<string, float> BiomeEncoding { get; private set; }
    public int ContextLength { get; private set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value [...]: only called by LoadAsync and BuildAsync,
    // which definitely initialize Data
    private MulticlassStringGenerator(VectorEncoding<string, float> biomeEncoding)
#pragma warning restore CS8618
    {
        Pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label", "Successor")
                                                   .Append(_mlContext.Transforms.Categorical.OneHotEncoding("BiomeEncoded", "Biome"))
                                                   .Append(_mlContext.Transforms.Categorical.OneHotEncoding("ContextEncoded", "Context"))
                                                   .Append(_mlContext.Transforms.Concatenate("Features", "BiomeEncoded", "ContextEncoded"));
        BiomeEncoding = biomeEncoding;
    }
    public async Task SaveAsync(string name = "multiclass.zip")
        => await Task.Run(() => _mlContext.Model.Save(Model, Data.Schema, name));
    public static MulticlassStringGenerator Load(string path)
    {
        // todo: change this to load from model.zip, you idiot
        // eh, too lazy (for now?)
        throw new NotImplementedException();
    }
    public static MulticlassStringGenerator Build(IEnumerable<(string item, CityInfo metadata)> corpus, int contextLength = Defaults.CONTEXT_LENGTH)
    {
        VectorEncoding<string, float> biomeEncoding = VectorEncoding<string, float>.From(corpus.Select(x => x.metadata.Biome));
        return BuildInternal(MulticlassFeatures.From(corpus, biomeEncoding, contextLength), biomeEncoding);
    }
    public static MulticlassStringGenerator BuildInternal(IEnumerable<MulticlassFeatures> data, VectorEncoding<string, float> biomeEncoding)
    {
        MulticlassStringGenerator result = new(biomeEncoding);
        result.Data = result._mlContext.Data.LoadFromEnumerable(data.ToList());
        return result;
    }
    public CharacterPrediction Predict(MulticlassFeatures input)
        => PredictionEngine.Predict(input);
    public string RandomChar(MulticlassFeatures input)
        => KeyValueMapper[Predict(input).CharacterWeights.WeightedRandomIndex() + 1];
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
    public string RandomString(MulticlassFeatures input, int minLength = 1, int maxLength = 100)
    {
        string[] ancestors = input.Ancestors;
        string result = $"{input.Ancestors.Join()}{input.Result}";
        int ct = 0;
        while (++ct < maxLength)
        {
            CharacterPrediction prediction = Predict(MulticlassFeatures.Query(input.BiomeWeights, ancestors));
            IEnumerable<float> normalizedWeights = prediction.CharacterWeights.Select(x => x * x);
            IEnumerable<(int index, float weight)> weightedIndices = 0.To(normalizedWeights.Count())
                                                                      .Select(x => x + 1)
                                                                      .Zip(normalizedWeights);
            // reasoning: when there are a lot of good choices, choose more of them
            float threshold = normalizedWeights.Median((x, y) => (x + y) / 2) / normalizedWeights.Average();
            weightedIndices = weightedIndices.Where(x => x.weight > threshold && (ct >= minLength || !KeyValueMapper[x.index].Contains(Characters.STOP)));
            if (!weightedIndices.Any())
                break;
            /*
            string weightString  = weightedIndices.OrderByDescending(x => x.weight)
                                                  .Select(x => $"{KeyValueMapper[x.index]}/{(int)KeyValueMapper[x.index][0]}: {x.weight,7:F4}")
                                                  .ListNotation();
            Console.WriteLine($"{result,20} + {weightString} (threshold: {threshold})");
            */
            string next = KeyValueMapper[weightedIndices.WeightedRandomElement(x => x.weight).index];
            if (next.Contains(Characters.STOP))
                break;
            result += next;
            ancestors = ancestors.FifoPush(next);
        }
        return result;
    }
    public string RandomString(CityInfo query, int minLength = 1, int maxLength = 100)
        => RandomString(MulticlassFeatures.Query(BiomeEncoding.Encode(query.Biome)), minLength, maxLength);
}