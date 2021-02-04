using System;
using System.Linq;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using System.Collections.Generic;

namespace mlnet_BTCTimeSeries
{
    class Program
    {
        static MLContext mlContext = new MLContext();
        static DateTime lastTime;
        static List<BTCDataModel> trainingData;
        static List<BTCDataModel> testingData;
        static string fileName = "btcModel.zip";
        static int horizon = 5;
        static void Main(string[] args)
        {
            GetTrainingData();
            TrainModel();
            Predict();
        }

        static void GetTrainingData()
        {
            IDataView trainingDataFile = mlContext.Data.LoadFromTextFile<BTCDataModel>("btc.csv", hasHeader: false, separatorChar: ',');

            //todo: get training and testing data
            List<BTCDataModel> data = mlContext.Data.CreateEnumerable<BTCDataModel>(trainingDataFile, false, true).ToList();

            //times in the data set are not uniform, so we will pull unique time values
            data = data.OrderBy( o => o.TimeStamp).ToList();

            int dataSubset = data.Count() - horizon;
            

            trainingData = data.GetRange(0, dataSubset).ToList(); //create our training data up to the dates we are trying to predict
            testingData = data.GetRange(dataSubset, horizon);// will get the number of items we are trying to predict
            
            Console.WriteLine(data.Count);
            
            lastTime = ConvertTimeStamp(trainingData.Last().TimeStamp);
        }

        static void TrainModel()
        {
            IDataView trainingDataView = mlContext.Data.LoadFromEnumerable<BTCDataModel>(trainingData);
            

            int seriesLength = trainingData.Count()-horizon;

            var estimator = mlContext.Forecasting.ForecastBySsa(outputColumnName: nameof(PredictedSeriesDataModel.ForecastedPrice),
                            inputColumnName: nameof(BTCDataModel.Price),
                            windowSize: 96, //intervals to measure data 96 is equivalent to 24 hours in 15 minute increments giving us 96 datapoints
                            seriesLength: seriesLength,
                            trainSize: seriesLength,
                            horizon: horizon,
                            confidenceLevel: 0.95f,
                            confidenceLowerBoundColumn: nameof(PredictedSeriesDataModel.ConfidenceLowerBound),
                            confidenceUpperBoundColumn: nameof(PredictedSeriesDataModel.ConfidenceUpperBound)
            );

            ITransformer forecastTransformer = estimator.Fit(trainingDataView);
            SaveModel(forecastTransformer, trainingDataView);
            
        }

        static void Predict()
        {
            ITransformer forecastTransformer = LoadModel();
             TimeSeriesPredictionEngine<BTCDataModel, PredictedSeriesDataModel> forecastEngine = forecastTransformer.CreateTimeSeriesEngine<BTCDataModel, PredictedSeriesDataModel>(mlContext);

             PredictedSeriesDataModel predictions = forecastEngine.Predict();
             for(int i = 0; i < predictions.ForecastedPrice.Count(); i++)
            {
                Console.WriteLine(ConvertTimeStamp(testingData[i].TimeStamp));
                lastTime = lastTime.AddMinutes(15);
                Console.WriteLine("{0} price: {1}, low: {2}, high: {3}, actual: {4}", lastTime, predictions.ForecastedPrice[i].ToString(), predictions.ConfidenceLowerBound[i].ToString(), predictions.ConfidenceUpperBound[i].ToString(), testingData[i].Price);
            }

            forecastEngine.CheckPoint(mlContext, fileName); //checkpoint updates the model instead of overwriting it. this makes time series training quicker as the data set grows
        }

        static void SaveModel(ITransformer Model, IDataView DataView)
        {
            mlContext.Model.Save(Model, DataView.Schema, fileName);
        }

        static ITransformer LoadModel()
        {
            DataViewSchema modelSchema;
            //gets a file from a stream, and loads it
            using(Stream s = File.Open(fileName, FileMode.Open))
            {
                return mlContext.Model.Load(s, out modelSchema);
            }
        }

        static DateTime ConvertTimeStamp(double TimeStamp)
        {
            var offset = TimeSpan.FromSeconds(TimeStamp);
            DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return startTime.Add(offset).ToLocalTime();
        }
    }
}
