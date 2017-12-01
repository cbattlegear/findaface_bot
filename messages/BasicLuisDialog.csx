#load "Cosmos.csx"

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

// For more information about this template visit http://aka.ms/azurebots-csharp-luis

[Serializable]
public class BasicLuisDialog : LuisDialog<object>
{
    public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(Utils.GetAppSetting("LuisAppId"), Utils.GetAppSetting("LuisAPIKey"))))
    {
    }

    [LuisIntent("None")]
    public async Task NoneIntent(IDialogContext context, LuisResult result)
    {
        await context.PostAsync($"You have reached the none intent. You said: {result.Query}"); //
        context.Wait(MessageReceived);
    }

    // Go to https://luis.ai and create a new intent, then train/publish your luis app.
    // Finally replace "MyIntent" with the name of your newly created intent in the following handler
    [LuisIntent("Find Picture")]
    public async Task FindPicture(IDialogContext context, LuisResult result)
    {
        await context.PostAsync($"You have reached the Find Picture intent. You said: {result.Query}"); //
        EntityRecommendation gender;
        if(result.TryFindEntity("gender", out gender)) {
            var our_gender = "";
            char[] charsToTrim = { '[', ' ', ']', '"' };
            foreach (var value in gender.Resolution.Values)
            {
                our_gender = Regex.Replace(our_gender, @"[^\u0000-\u007F]+", string.Empty);
                our_gender = value.ToString().Trim(charsToTrim);
            }
            await context.PostAsync($"You sent the Gender: {our_gender}");

            try
            {
                Cosmos c = new Cosmos();
                c.OpenConnection().Wait();
                List<string> thumbnails = await c.ExecuteSimpleQuery("c.faceAttributes.gender = '" + our_gender + "'", context);

                foreach(string thumbnail in thumbnails)
                {
                    var message = context.MakeMessage();

                    var attachment = GetHeroCard(thumbnail);
                    message.Attachments.Add(attachment);

                    await context.PostAsync(message);
                }
            }
            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message, baseException.Message);
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
        }
        context.Wait(MessageReceived);
    }

    private static Microsoft.Bot.Connector.Attachment GetHeroCard(string image_url)
    {
        var heroCard = new HeroCard
        {
            Title = "BotFramework Thumbnail Card",
            Subtitle = "Your bots — wherever your users are talking",
            Text = "Build and connect intelligent bots to interact with your users naturally wherever they are, from text/sms to Skype, Slack, Office 365 mail and other popular services.",
            Images = new List<CardImage> { new CardImage(image_url) },
            Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Get Started", value: "https://docs.microsoft.com/bot-framework") }
        };

        return heroCard.ToAttachment();
    }
}