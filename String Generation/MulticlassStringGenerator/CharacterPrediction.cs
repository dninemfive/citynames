using Microsoft.ML.Data;

namespace citynames;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor:
                               // Initialized by ML.NET
public class CharacterPrediction
{
    [ColumnName("Score")]
    public float[] CharacterWeights;
    [ColumnName("PredictedLabel")]
    public string PredictedCharacter;
    public override string ToString()
        => $"CharacterPrediction({PredictedCharacter}, {CharacterWeights})";
}