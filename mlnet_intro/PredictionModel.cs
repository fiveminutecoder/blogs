using Microsoft.ML.Data;

namespace mlnet_intro
{
    public class PredictionModel
    {
        [ColumnName("PredictedSpecies")]
        public string PredictedSpecies {get;set;}
        [ColumnName("Score")]
        public float[] Score {get;set;}
    }
}