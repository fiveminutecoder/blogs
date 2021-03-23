using System;
using System.Web;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Collections.Generic;

namespace SharePointMigration_WebJob
{
    class Program
    {
        //making this a property so we do not have to keep recreating the object
        static QueueClient queue; 
        //connection string to our storage account, it is shared between blobs and queues.
        static string cs = "<Storage Account Connection String>";

        //The site ID our library lives in
        static string siteId = "";
        //The ID of the library
        static string libraryId = "";
        //The ID of the root folder in the library
        static string rootFolderId = "";
        static async Task Main(string[] args)
        {
            queue = new QueueClient(cs, "customer");
            int batchMessageCount = 30; //number of items to pull from queue at once
            int queueLock = batchMessageCount * 10; //number of batches time 10 since each message will take 10 seconds to process.

            //
            await GetSiteandListIDs();

            //Creating an infinite loop for our continuous job
            while(true)
            {
                DateTime startTime = DateTime.Now;
                try
                {
                    Console.WriteLine("Getting queue");
                    QueueMessage[] messages = await GetFileMetaDataFromQueue(batchMessageCount, queueLock);
                    Console.WriteLine("Found {0} items in the queue", messages.Length);
                    foreach(QueueMessage message in messages)
                    {
                        //our cleint job encoded the message, this will decode it
                        string data = HttpUtility.UrlDecode(message.MessageText);
                        CustomerMetadata customer = JsonConvert.DeserializeObject<CustomerMetadata>(data);

                        Console.WriteLine("Pulling document {0}", customer.FileName);
                        using(MemoryStream document = await GetFileFromAzureBlob(customer.FileName))
                        {
                            Console.WriteLine("Uploading document {0}", customer.FileName);
                            await UploadDocumentToSharePoint(document, customer);
                        }

                        Console.WriteLine("Upload was successful, removing {0} from the queue", customer.FileName);
                        //remove message from queue so it doesnt get pulled again since we were successful
                        await RemoveItemFromQueue(message.MessageId, message.PopReceipt);
                        //remove document from storage
                        await RemoveDocumentFromQueue(customer.FileName);

                        //sleep 10 seconds before next call to sharepoint to prevent throttling.
                        System.Threading.Thread.Sleep(10000);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error writing queue to SharePoint: " + ex.Message);
                }

                Console.WriteLine("Finished with current queue list, will wait 5 minutes from last call");
                //we want our job to sleep if it takes less than 5 minutes to process the queue. This is to prevent throttling
                DateTime endTime = DateTime.Now;
                double totalMinutes = endTime.Subtract(startTime).TotalMinutes;

                if(totalMinutes < 5)
                {
                    double sleepTime = (5-totalMinutes) * 60000;
                    System.Threading.Thread.Sleep(Convert.ToInt32(sleepTime));
                }
            }
            
        }

        ///<summary>
        ///Pulls data from Azure Queue
        ///</summary>
        ///<param name="MessageCount">The number of messages in the queue to pull</param>
        ///<param name="QueueLock">This locks the messages so they cannot be pulled again for X number of seconds</param>
        public static async Task<QueueMessage[]> GetFileMetaDataFromQueue(int MessageCount=1, int QueueLock=60)
        {
            //calls the queue and pulls messages for processing
            QueueMessage[] queueMessages = await queue.ReceiveMessagesAsync(MessageCount, TimeSpan.FromSeconds(QueueLock));
            
            return queueMessages;
        }

        ///<summary>
        ///get memory stream of file to upload to SharePoint
        ///</summary>
        ///<param name="FileName">Name of file to pull from queue</param>
        public static async Task<MemoryStream> GetFileFromAzureBlob(string FileName)
        {
            BlobClient blobClient = new BlobClient(cs, "customer", FileName);

            using (BlobDownloadInfo downloadInfo = await blobClient.DownloadAsync())
            {
                MemoryStream stream = new MemoryStream();
                
                downloadInfo.Content.CopyTo(stream);
                return stream;
                
            }
        }

        
        ///<summary>
        ///Create an authentication token for our graph api based on our Azure AD app
        ///</summary>
        public static async Task<string> GetGraphAPIToken()
        {
            string tenantId = "<TenantID found in Azure>"; //realm
            //some service account to upload docs. Documents cannot use app
            string clientId = "<ClientID for our app>"; 
            //service account password
            string clientSecret = "<Client Secret for our app>"; 
            string[] scopes = new string[] {"https://graph.microsoft.com/.default" };
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(clientId)
                        .WithClientSecret(clientSecret)
                        .WithAuthority(new Uri("https://login.microsoftonline.com/" + tenantId))
                        .Build();

                AuthenticationResult result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

                //result contains an ExpiresOn property which can be used to cache token
                return result.AccessToken;
        }

        ///<summary>
        ///Graph call to get our site, library, and root folder id.
        ///We call this at the beginning of our app since it is only needed once
        ///</summary>
        public static async Task GetSiteandListIDs()
        {
            //Crate our graph client
            GraphServiceClient graphClient = new GraphServiceClient("https://graph.microsoft.com/v1.0", new DelegateAuthenticationProvider(
                async(requestMessage) =>{
                    string token = await GetGraphAPIToken();
                    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            )); 

            //Gets the root site, if your library lives somewhere else you will need the collection and find it.
            var sites = await graphClient.Sites.Root.Request().GetAsync();
            siteId = sites.Id;

            //gets all libraries. Since our app is written in 5 minutes it is easier to filter the entire collection
            var libraries = await graphClient.Sites[siteId].Drives.Request().GetAsync();
            var library = libraries.First(f => f.Name == "CustomerDocs");

            libraryId = library.Id;

            // gets root folder of our library
            var rootFolder = await graphClient.Sites[siteId].Drives[libraryId].Root.Request().GetAsync();

            rootFolderId = rootFolder.Id;
        }

        ///<summary>
        ///Uploads document to sharepoint and tags it
        ///</summary>
        ///<param name="FileStream">Memory Stream containing file to be uploaded</param>
        ///<param name="Metadata">Our customer metadata</param>
        public static async Task UploadDocumentToSharePoint(Stream FileStream, CustomerMetadata Metadata)
        {
            //Gets graph client. We get this each time to make sure our token is not expired
            GraphServiceClient graphClient = new GraphServiceClient("https://graph.microsoft.com/v1.0", new DelegateAuthenticationProvider(
                async(requestMessage) =>{
                    string token = await GetGraphAPIToken();
                    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            )); 

            //Uploads our file to our library
            DriveItem createDocument = await graphClient.Sites[siteId].Drives[libraryId].Items[rootFolderId].ItemWithPath(Metadata.FileName).Content.Request().PutAsync<DriveItem>(FileStream);
            
            //Our metadata for our document
            FieldValueSet custData = new FieldValueSet{
                AdditionalData = new Dictionary<string, object>()
                {
                    {"State", Metadata.State},
                    {"City", Metadata.City},
                    {"AccountNumber", Metadata.AccountNumber}
                }
            };

            //sets the metada properites for our item
            await graphClient.Sites[siteId].Drives[libraryId].Items[rootFolderId].ItemWithPath(Metadata.FileName).ListItem.Fields.Request().UpdateAsync(custData);

            //try checking in file, some libraries it is required 
            try
            {
                await graphClient.Sites[siteId].Drives[libraryId].Items[createDocument.Id].Checkin().Request().PostAsync();
            }
            catch(Exception ex){
                //ignoring this error becuase library is not set for checkin/out
                if(!ex.Message.Contains("The file is not checked out"))
                {
                    throw ex;
                }
            }
        }

        ///<summary>
        ///Removes item from our queue
        ///</summary>
        ///<param name="MessageId">Guid of the message</param>
        ///<param name="Receipt">Tracks what call is removing the item from the Queue</param>
        public static async Task RemoveItemFromQueue(string MessageId, string Receipt)
        {
            await queue.DeleteMessageAsync(MessageId, Receipt);
        }

        ///<summary>
        ///Removes document from blob storage
        ///</summary>
        ///<param name="FileName">Name of the file to be removed</param>
        public static async Task RemoveDocumentFromQueue(string FileName)
        {
            BlobClient blobClient = new BlobClient(cs, "customer", FileName);
            await blobClient.DeleteAsync();
        }
    }
}
