using Microsoft.ML.Data;

namespace mlnet_BTCTimeSeries
{
    public class BTCDataModel
    {
        [LoadColumn(0)]
        public int TimeStamp {get;set;}

        [LoadColumn(1)]
        public float Open {get;set;}
        [LoadColumn(2)]
        public float High {get;set;}
        [LoadColumn(3)]
        public float Low {get;set;}
        [LoadColumn(4)]
        public float Close {get;set;}

        [LoadColumn(5)]
        public float Volume {get;set;}
        [LoadColumn(6)]
        public float Currency {get;set;}

        [LoadColumn(7)]
        public float Amount {get;set;}
    }
}