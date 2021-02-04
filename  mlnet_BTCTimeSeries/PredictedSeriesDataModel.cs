using Microsoft.ML.Data;

namespace mlnet_BTCTimeSeries
{
    public class PredictedSeriesDataModel
    {
        public float[] ForecastedPrice { get; set; }
        public float[] ConfidenceLowerBound { get; set; }
        public float[] ConfidenceUpperBound { get; set; }
    }
}