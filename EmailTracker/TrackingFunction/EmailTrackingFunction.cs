using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace FiveMinuteBlogger.EmailTracker
{
    public static class EmailTrackingFunction
    {
        [Function("EmailTrackingFunction")]
        public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("EmailTrackingFunction");
            

            // create connection to local settings
            var config = new ConfigurationBuilder()
                        .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .Build();
            string cs = config.GetConnectionString("EmailTrackingDB");

            //connect to sql db
            using(SqlConnection connection = new SqlConnection(cs))
            {

                //insert our tracking data into our database
                var queryStrings = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
                connection.Open();
                using(SqlCommand cmd = new SqlCommand("INSERT INTO campaign_tracking (TrackingId, Email, OpenDate, Campaign) Values(@TrackingId, @Email, @OpenDate, @Campaign)", connection))
                {
                    cmd.Parameters.AddWithValue("@TrackingId", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("@Email", queryStrings.Get("Email").ToString());//email that opened link
                    cmd.Parameters.AddWithValue("@OpenDate",DateTime.Now); //time email was opened
                    cmd.Parameters.AddWithValue("@Campaign", queryStrings.Get("Campaign").ToString()); //ID linked to another table with campaign details

                    cmd.ExecuteNonQuery();

                    //log that email entry was made
                    logger.LogInformation("Logged {0}", queryStrings.Get("Email").ToString());
                }

                connection.Close();
            }

            
           //create a successful response to return in the function
            var response = req.CreateResponse(HttpStatusCode.OK);
           
            return response;
        }
    }
}
