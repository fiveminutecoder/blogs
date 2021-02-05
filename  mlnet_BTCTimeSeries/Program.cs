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
        //last time to show the times of our future predictions
        static DateTime lastTime; 
        static List<BTCDataModel> trainingData;
        static List<BTCDataModel> testingData;
        static string fileName = "btcModel.zip";
        //how far out we want to predict
        static int horizon = 5; 
        //holds or in memory model
        static ITransformer forecastTransformer; 
        static void Main(string[] args)
        {
            GetTrainingData();
            TrainModel();
            Predict();
        }

        static void GetTrainingData()
        {
            //load our dataset
            IDataView trainingDataFile = mlContext.Data.LoadFromTextFile<BTCDataModel>("bitstampUSD_1-min_data_2012-01-01_to_2020-12-31.csv", hasHeader: true, separatorChar: ',');

            //create enumerable to manipulate data
            List<BTCDataModel> data = mlContext.Data.CreateEnumerable<BTCDataModel>(trainingDataFile, false, true).ToList();

            //times in the data set are not uniform, so we will pull unique time values
            data = data.OrderBy( o => o.TimeStamp).ToList();

            //determines the size of our testing data
            int dataSubset = data.Count() - horizon;

            //create our training data up to the dates we are trying to predict
            trainingData = data.GetRange(0, dataSubset).ToList(); 

            // will get the number of items we are trying to predict
            testingData = data.GetRange(dataSubset, horizon);
            
            //We want to capture time of last item in training data so we can increment the time stamp for our output and put a date/time to the forecast
            lastTime = ConvertTimeStamp(trainingData.Last().TimeStamp);
        }

        static void TrainModel()
        {
            IDataView trainingDataView = mlContext.Data.LoadFromEnumerable<BTCDataModel>(trainingData);

            // creates our estimater, as you cans see we are using forecasting estimator
            var estimator = mlContext.Forecasting.ForecastBySsa(outputColumnName: nameof(PredictedSeriesDataModel.ForecastedPrice),
                            inputColumnName: nameof(BTCDataModel.Amount), //column used for time series prediction
                            windowSize: 60, //series is sampled in 60 minute windows or periods, and the past 60 minutes will be used to make the prediction
                            seriesLength: 1440, //we want to train over a day's worth of time so this will be the interval, we have 1440 minutes in a day
                            trainSize: trainingData.Count(), //how many data points we want to sample
                            horizon: horizon,
                            confidenceLevel: 0.45f, //sets our margin of error, lower the confidence level the smaller the upper/lower bounds
                            confidenceLowerBoundColumn: nameof(PredictedSeriesDataModel.ConfidenceLowerBound),
                            confidenceUpperBoundColumn: nameof(PredictedSeriesDataModel.ConfidenceUpperBound)
            );

            //creates our fitted model
            forecastTransformer = estimator.Fit(trainingDataView); 
        }

        static void Predict()
        {
            //prediction engine based on our fitted model
             TimeSeriesPredictionEngine<BTCDataModel, PredictedSeriesDataModel> forecastEngine = forecastTransformer.CreateTimeSeriesEngine<BTCDataModel, PredictedSeriesDataModel>(mlContext);

            //call to predict the next 5 minutes
             PredictedSeriesDataModel predictions = forecastEngine.Predict();

             //write our predictions
             for(int i = 0; i < predictions.ForecastedPrice.Count(); i++)
            {   
                lastTime = lastTime.AddMinutes(1);
                Console.WriteLine("{0} price: {1}, low: {2}, high: {3}, actual: {4}", lastTime, predictions.ForecastedPrice[i].ToString(), predictions.ConfidenceLowerBound[i].ToString(), predictions.ConfidenceUpperBound[i].ToString(), testingData[i].Amount);
            }


            //instead of saving, we use checkpoint. This allows us to continue training with updated data and not need to keep such a large data set
            //so we can append Jan. 2021 without having everythign before to train the model speeding up the process
            forecastEngine.CheckPoint(mlContext, fileName);
        }


        //helper for converting timestamp to date time
        static DateTime ConvertTimeStamp(double TimeStamp)
        {
            var offset = TimeSpan.FromSeconds(TimeStamp);
            DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return startTime.Add(offset).ToLocalTime();
        }
    }
}
