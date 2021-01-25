using Microsoft.ML.Data;


namespace mlnet_NLP
{
    public class FAQModel
    {
        [ColumnName("Question"), LoadColumn(0)]
        public string Question {get;set;}
        [ColumnName("Answer"), LoadColumn(1)]
        public string Answer {get;set;}
    }
}