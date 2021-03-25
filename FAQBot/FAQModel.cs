using Microsoft.ML.Data;


namespace EchoBot.Bots
{
    public class FAQModel
    {
        [ColumnName("Question"), LoadColumn(0)]
        public string Question {get;set;}
        [ColumnName("Answer"), LoadColumn(1)]
        public string Answer {get;set;}
    }
}