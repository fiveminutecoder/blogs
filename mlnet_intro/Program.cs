using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.ML;

namespace mlnet_intro
{
    class Program
    {
        //Main context
        static MLContext context;
        //model for training/testing
        static Microsoft.ML.Data.TransformerChain<Microsoft.ML.Transforms.KeyToValueMappingTransformer> model;
        static IEnumerable<IrisModel> trainingData;
        static IEnumerable<IrisModel> testingData;
        
        static string fileName = "irisModel.zip";
        static void Main(string[] args)
        {
            Console.WriteLine("Training Iris Model");
            TrainModel();
            Console.WriteLine("Testing Iris Model");
            TestModel();
            SaveModel();

            IrisModel test = new IrisModel(){
                    SepalLengthCm = 5.2f,
                    SepalWidthCm = 3.5f,
                    PetalLengthCm = 1.4f,
                    PetalWidthCm = 0.2f,
                    Species = "hello"
                };

            Predict(test);

            Console.Read();
        }

        static void TrainModel()
        {
            context = new MLContext();

            //Load data from csv file
            var data = context.Data.LoadFromTextFile<IrisModel>("datasets_19_420_Iris.csv", hasHeader:true, separatorChar: ',', allowQuoting: true, allowSparse:true, trimWhitespace: true);
            
            //Splits data into training and testing data
            //Id is the unique key to keep labels from duplicating
            var split = context.Data.TrainTestSplit(data);
            
             
            //create data sets for trainiing and testing
            trainingData = context.Data.CreateEnumerable<IrisModel>(split.TrainSet, reuseRowObject: false);
            testingData = context.Data.CreateEnumerable<IrisModel>(split.TestSet, reuseRowObject: false);


            //Create our pipeline and set our training model
            var pipeline = context.Transforms.Conversion.MapValueToKey(outputColumnName: "Label", "Species") //converts string to key value for training
                .Append(context.Transforms.Concatenate("Features", new[]{"SepalLengthCm", "SepalWidthCm", "PetalLengthCm", "PetalWidthCm"})) //identifies training data from model
                .Append(context.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName: "Label", featureColumnName: "Features")) //set trainer and identifies features and label
                .Append(context.Transforms.Conversion.MapKeyToValue(outputColumnName: "PredictedSpecies", inputColumnName: "PredictedLabel")); //convert prediction to string PredictedLabel is output label key for predict

            //traings the model
             model = pipeline.Fit(context.Data.LoadFromEnumerable(trainingData));



        }

        static void TestModel()
        {
            //transform data to a view that can be evaluated
            IDataView testDataPredictions = model.Transform(context.Data.LoadFromEnumerable(testingData));
            //evaluate test data against trained model for accuracy
            var metrics = context.MulticlassClassification.Evaluate(testDataPredictions);
            double accuracy = metrics.MicroAccuracy;

            Console.WriteLine("Accuracy {0}", accuracy.ToString());

        }

        static void Predict(IrisModel iris)
        {
            ITransformer trainedModel = LoadModel();

            //Creates prediction function from loaded model, you can load in memory model as wwell
             var predictFunction = context.Model.CreatePredictionEngine<IrisModel, PredictionModel>(trainedModel);
             
            //pass model to function to get prediction outputs
            PredictionModel prediction = predictFunction.Predict(iris);

            //get score, score is an array and the max score will align to key.
            float score = prediction.Score.Max();
        
            Console.WriteLine("Prediction: {0},  accuracy: {1}", prediction.PredictedSpecies, score);

        }

        static void SaveModel()
        {
            IDataView dataView = context.Data.LoadFromEnumerable<IrisModel>(trainingData);
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
