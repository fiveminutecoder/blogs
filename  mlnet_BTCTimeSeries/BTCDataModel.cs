using Microsoft.ML.Data;

namespace mlnet_BTCTimeSeries
{
    public class BTCDataModel
    {
        [LoadColumn(0)]
        public int TimeStamp {get;set;}

        [LoadColumn(1)]
        public float Price {get;set;}
        [LoadColumn(2)]
        public float Amount {get;set;}
    }
}