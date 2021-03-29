// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with EchoBot .NET Template version v4.12.2
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.ML;

namespace EchoBot.Bots
{
    public class FAQBot : ActivityHandler
    {
        //context or our machine learning model
        static MLContext context;
        //used to read and predict our questions
        static PredictionEngine<FAQModel, PredictionModel> predictionEngine;

        //creating a private staitc constructor
        //Our model is not changing so it doesnt make sense to keep opening it and reading the zip for performance
        static FAQBot()
        {
            //structure of our data model
            DataViewSchema modelSchema;
            //the model loaded for prediction
            ITransformer trainedModel;
            context = new MLContext();

            //load our file
            using(Stream s = File.Open("FAQModel.zip", FileMode.Open))
            {
                trainedModel = context.Model.Load(s, out modelSchema);
            }

            //creates our prediction engine
            predictionEngine = context.Model.CreatePredictionEngine<FAQModel, PredictionModel>(trainedModel);

        }
        
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //creates our FAQ model
            FAQModel question = new FAQModel()
            {
                Question = turnContext.Activity.Text,//gets text from bot
                Answer = ""
            };

            //uses our trained model to predict our answer
            PredictionModel prediction = FAQBot.predictionEngine.Predict(question);

            //accuracy of prediction
            float score = prediction.Score.Max() * 100;

            //gonna check if we were accurate,if below a threshold we will ask them to clarify
            if(score > 60)
            {
                //sends our answer back to the bot
                await turnContext.SendActivityAsync(MessageFactory.Text(prediction.PredictedAnswer), cancellationToken);
                await turnContext.SendActivityAsync(MessageFactory.Text($"We think our answer to your question is this accurate: {score}%"), cancellationToken);
            }
            else
            {
                //sends them suggestions that are clickable
                await turnContext.SendActivityAsync(MessageFactory.Text("Sorry, we didnt understand the question, please try selecting a question below"), cancellationToken);
                string[] actions = {"What are your hours?","How can I reach you?", "What payments do you accept?"};
                await turnContext.SendActivityAsync(MessageFactory.SuggestedActions(actions), cancellationToken);
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
