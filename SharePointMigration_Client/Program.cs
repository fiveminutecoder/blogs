using System;
using System.IO;
using System.Collections.Generic;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Newtonsoft.Json;
using System.Web;
using System.Threading.Tasks;

namespace SharePointMigration_Client
{
    
    class Program
    {
        //making this a property so we do not have to keep recreating the object
        static QueueClient queue; 
        //connection string to our storage account, it is shared between blobs and queues.
        static string cs = "DefaultEndpointsProtocol=https;AccountName=fiveminute;AccountKey=M2Ezz9UiPkfg/voWgM3xgDeUek7mpsEuujiKXvgf69Y1/ikheLFP6opQ0jwQBTkd6zxop7ZY+MNZvsR2buzb3g==;EndpointSuffix=core.windows.net";

        static async Task Main(string[] args)
        {
            
            //create our queue client
            queue = new QueueClient(cs, "customer"); //customer is the name of our queue where we can view the items uploaded in Azure
            
            await GetFilesInDirectory("c:\\customers");
            Console.Read();
        }

        //Recurssive function for iterating directory and sub directories
        public static async Task GetFilesInDirectory(string FileDirectory)
        {
            Console.WriteLine("Looking for files in " + FileDirectory);
            string[] files = Directory.GetFiles(FileDirectory);

            foreach(string file in files)
            {
                Console.WriteLine(file);
                await UploadFileToAzureBlob(file);
                await UploadFileMetaDataToQueue(file);
            }

            string[] subdirectories = Directory.GetDirectories(FileDirectory);

            foreach(string subdirectory in subdirectories)
            {
                //Recursion for going thorugh all directories
                await GetFilesInDirectory(subdirectory);
            }
        }

        public static async Task UploadFileToAzureBlob(string FilePath)
        {
            string fileName = Path.GetFileName(FilePath);
            Console.WriteLine("File name {0}", fileName);
            //customer is the name of our blob container where we can view documents in Azure
            //blobs require us to create a connection each time we want to upload a file
            BlobClient blob  = new BlobClient(cs, "customer", fileName); 

            //Gets a file stream to upload to Azure
            using(FileStream stream = File.Open(FilePath, FileMode.Open))
            {
                await blob.UploadAsync(stream);
            }
        }

        public static async Task UploadFileMetaDataToQueue(string FilePath)
        {
            string[] metadata = FilePath.Split('\\');

            //we know our metadata position because of our file structure. 
            CustomerMetadata customerMetadata = new CustomerMetadata()
            {
                State = metadata[2],
                City = metadata[3],
                AccountNumber = metadata[4],
                FileName = metadata[5]
            };

            //create a string for our queue
            string data = JsonConvert.SerializeObject(customerMetadata); 
            //We do this to ensure any non UTC-8 characters are safe for the web service
            data = HttpUtility.UrlEncode(data); 

            //adds message to back of the queue
            await queue.SendMessageAsync(data);
        }
    }
}
