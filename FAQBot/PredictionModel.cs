using Microsoft.ML.Data;

namespace EchoBot.Bots
{
    public class PredictionModel
    {
        [ColumnName("PredictedAnswer")]
        public string PredictedAnswer {get;set;}
        [ColumnName("Score")]
        public float[] Score {get;set;}
    }
}