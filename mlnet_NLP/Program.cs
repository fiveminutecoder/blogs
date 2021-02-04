using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.ML;

namespace mlnet_NLP
{
    class Program
    {
        //Main context
        static MLContext context;
        //model for training/testing
        static Microsoft.ML.Data.TransformerChain<Microsoft.ML.Transforms.KeyToValueMappingTransformer> model;
        static IEnumerable<FAQModel> trainingData;
        static IEnumerable<FAQModel> testingData;
        static string fileName = "FAQModel.zip";

        static void Main(string[] args)
        {
            TrainModel();
            TestModel();
            SaveModel();
            FAQModel question = new FAQModel(){
                Question = "can i Pay online?",
                Answer = ""
            };

            Predict(question);
        }

        static void TrainModel()
        {
            context = new MLContext();

            //Load data from csv file
            var data = context.Data.LoadFromTextFile<FAQModel>("faq.csv", hasHeader:true, separatorChar: ',', allowQuoting: true, allowSparse:true, trimWhitespace: true);
            
  
            //create data sets for trainiing and testing
            trainingData = context.Data.CreateEnumerable<FAQModel>(data, reuseRowObject: false);
            testingData = new List<FAQModel>()
            {
                new FAQModel() {Question = "When are you open?", Answer = "Our hours are 9 am to 5pm Monday through Friday"},
                new FAQModel() {Question = "Can i pay using a visa card?", Answer =  "Our payment options are Credit, Check, or Bitcoin"},
                new FAQModel() {Question = "How can i contact you.", Answer = "Our phone number is 555-5555 and our fax is 555-5557"}
            };

            //Create our pipeline and set our training model
            var pipeline = context.Transforms.Conversion.MapValueToKey(outputColumnName: "Label", inputColumnName: "Answer") //converts string to key value for training
                .Append(context.Transforms.Text.FeaturizeText( "Features","Question")) //creates features from our text string
                .Append(context.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName: "Label", featureColumnName: "Features"))//set up our model
                .Append(context.Transforms.Conversion.MapKeyToValue(outputColumnName: "PredictedAnswer", inputColumnName: "PredictedLabel")); //convert our key back to a label

            //traings the model
             model = pipeline.Fit(context.Data.LoadFromEnumerable(trainingData));
        }

        static void TestModel()
        {
            //transform data to a view that can be evaluated
            IDataView testDataPredictions = model.Transform(context.Data.LoadFromEnumerable(testingData));
            //evaluate test data against trained model for accuracy
            var metrics = context.MulticlassClassification.Evaluate(testDataPredictions);
            double accuracy = metrics.MacroAccuracy;

            Console.WriteLine("Accuracy {0}", accuracy.ToString());
        }

        static void Predict(FAQModel Question)
        {
            ITransformer trainedModel = LoadModel();

            //Creates prediction function from loaded model, you can load in memory model as wwell
             var predictFunction = context.Model.CreatePredictionEngine<FAQModel, PredictionModel>(trainedModel);
             
            //pass model to function to get prediction outputs
            PredictionModel prediction = predictFunction.Predict(Question);

            //get score, score is an array and the max score will align to key.
            float score = prediction.Score.Max();
        
            Console.WriteLine("Prediction: {0},  accuracy: {1}", prediction.PredictedAnswer, score);

        }

        static void SaveModel()
        {
            IDataView dataView = context.Data.LoadFromEnumerable<FAQModel>(trainingData);
           context.Model.Save(model, dataView.Schema, fileName);
        }

        static ITransformer LoadModel()
        {
            DataViewSchema modelSchema;
            //gets a file from a stream, and loads it
            using(Stream s = File.Open(fileName, FileMode.Open))
            {
                return context.Model.Load(s, out modelSchema);
            }
        }
    }
}
