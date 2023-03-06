using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Azure.Identity;


namespace FiveMinuteCode.Reply
{
    public class CheckNewEmail
    {
        //openAISecretKey
        private string openAIKey = "<OPEN AI API key here>";
        //ID of the mailbox you want to auto reply from
        private string userId = "user id of mailbox";
        //ID of the tenant used
        private string tenantId = "azure tenant"; 
        //App id from created azure app
        private string clientId = "registered app client id"; 
        //Secret created for the app
        private string clientSecret = "registered app secret"; 
        //hold our graph context here for our calls
        private GraphServiceClient graphService;


        [FunctionName("CheckNewEmail")]
        public async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer,  ILogger log)
        {
            try
            {
                log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
                graphService = GetGraphAPIClient();
                List<Message> newMessages = await GetNewEmails();
                log.LogInformation("found " + newMessages.Count);
                foreach(Message message in newMessages)
                {
                    log.LogInformation("replying to " + message.Subject);
                    string response = await GetChatGPTResponse(message.Subject, message.Body.Content);
                    await SendEmail(message.Id, message.From, response);
                    await UpdateToRead(message.Id);
                    log.LogInformation("Reply successful");

                }
            }
            catch(Exception ex)
            {
                log.LogError(ex, ex.Message);
            }
        }   

        //Create the graph service client  that will be used to get and respond to emails
        private GraphServiceClient GetGraphAPIClient()
        {
            
            string[] scopes = new string[] {"https://graph.microsoft.com/.default" };
            // using Azure.Identity;
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            ClientSecretCredential clientSecretCredential = new ClientSecretCredential(
                tenantId, clientId, clientSecret, options);

            GraphServiceClient graphClient = new GraphServiceClient(clientSecretCredential, scopes);
            return graphClient;
        }

        //Graph API call to get all unread emails
        private async Task<List<Message>> GetNewEmails()
        {
            MessageCollectionResponse messages = await graphService.Users[userId].Messages.GetAsync((requestConfiguration) =>{
                requestConfiguration.QueryParameters.Filter = "isRead eq false";
            });
            
            return messages.Value;
        }


        //The call to the Chat GPT end point
        private async Task<string> GetChatGPTResponse(string Subject, string Body)
        {
            OpenAI_API.OpenAIAPI openai = new OpenAI_API.OpenAIAPI(openAIKey);

            //Create a request suitable for the Chat GPT API. It will remove an non readable characters that the API cannot read
            OpenAI_API.Completions.CompletionRequest completionRequest = new OpenAI_API.Completions.CompletionRequest(Subject + "." + Body, OpenAI_API.Models.Model.CurieText,150);

            // Send a request to the ChatGPT model
            OpenAI_API.Completions.CompletionResult response = await openai.Completions.CreateCompletionAsync(completionRequest);
        
            return response.Completions[0].Text;
        }

        //Graph API call to send reply to email
        private async Task SendEmail(string MessageId, Recipient RecipientEmail, string Response)
        {
            Microsoft.Graph.Users.Item.Messages.Item.Reply.ReplyPostRequestBody reply = new Microsoft.Graph.Users.Item.Messages.Item.Reply.ReplyPostRequestBody
            {
                Message = new Message
                {
                    ToRecipients = new List<Recipient>
                    {
                        new Recipient()
                        {
                            EmailAddress = new EmailAddress()
                            {
                                Address = RecipientEmail.EmailAddress.Address,
                                Name = !String.IsNullOrEmpty(RecipientEmail.EmailAddress.Name) ? RecipientEmail.EmailAddress.Name : RecipientEmail.EmailAddress.Address
                            }
                        }
                    },
                },
                Comment = Response,
                
            };

            await graphService.Users[userId].Messages[MessageId].Reply.PostAsync(reply);
        }

        //Graph API call to update email to read
        private async Task UpdateToRead(string MessageId)
        {
            
            //only update the properties we want to update
            Message msg = new Message()
            {
                IsRead = true
            };
            await graphService.Users[userId].Messages[MessageId].PatchAsync(msg);
        }
    }
}
