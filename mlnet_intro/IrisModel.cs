using Microsoft.ML.Data;

namespace mlnet_intro
{
    public class IrisModel
    {
        [ColumnName("Id"), LoadColumn(0)]
        public int Id {get;set;}
        [ColumnName("SepalLengthCm"), LoadColumn(1)]
        public float SepalLengthCm {get;set;}
        [ColumnName("SepalWidthCm"), LoadColumn(2)]
        public float SepalWidthCm {get;set;}
        [ColumnName("PetalLengthCm"), LoadColumn(3)]
        public float PetalLengthCm {get;set;}
        [ColumnName("PetalWidthCm"), LoadColumn(4)]
        public float PetalWidthCm {get;set;}
        [ColumnName("Species"), LoadColumn(5)]
        public string Species {get;set;}
        
    }
}