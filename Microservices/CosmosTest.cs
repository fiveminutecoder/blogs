using System.Collections.Generic;
using System;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading.Tasks;

namespace FiveMinuteCoder
{
    public static class CosmosTest
    {
        [Function("CosmosTest")]
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("CosmosTest");
            logger.LogInformation("C# HTTP trigger function processed a request.");
            mongoObject requestData = await req.ReadFromJsonAsync<mongoObject>();
            string connectionString =@"mongodb://mongoservice:QwIpBF4HFfatf7pMyswMp9DwI09UtkJPJCQVQsoeza0uhIfPu8FHfT5QeeFjXmgnOHTcDarDVDQRjIXG9j0PjA==@mongoservice.mongo.cosmos.azure.com:10255/?ssl=true&retrywrites=false&replicaSet=globaldb&maxIdleTimeMS=120000&appName=@mongoservice@";
            MongoClientSettings settings = MongoClientSettings.FromUrl(
            new MongoUrl(connectionString)
            );
            settings.SslSettings = 
            new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            var mongoClient = new MongoClient(settings);

            var db = mongoClient.GetDatabase("test");

            mongoObject o = new mongoObject(){
                Id = new ObjectId(),
                Data = "Hi"
            };

            var collection = db.GetCollection<mongoObject>("test");
            
            collection.InsertOne(o);
            var filter= Builders<mongoObject>.Filter.Empty;
           var stuff = collection.Find(filter);
 
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString(stuff.First().Data);

            return response;
        }
    }

    public class mongoObject{
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id {get;set;}
        public string Data {get;set;}
    }
}
