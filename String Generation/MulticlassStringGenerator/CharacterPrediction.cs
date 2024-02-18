using Microsoft.ML.Data;

namespace citynames;
public class CharacterPrediction
{
    [ColumnName("Score")]
    public float[] CharacterWeights;
    [ColumnName("PredictedLabel")]
    public string PredictedCharacter;
    public override string ToString()
        => $"CharacterPrediction({PredictedCharacter}, {CharacterWeights})";
}
