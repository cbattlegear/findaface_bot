using System;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

// For more information about this template visit http://aka.ms/azurebots-csharp-luis

public class BasicLuisDialog : LuisDialog<object>
{
    private string EndpointUrl = Utils.GetAppSetting("CosmosEndpoint");
    private string PrimaryKey = Utils.GetAppSetting("CosmosPrimaryKey");
    private DocumentClient client;
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
            foreach (var value in gender.Resolution.Values)
            {
                our_gender = value.ToString();
            }
            await context.PostAsync($"You sent the Gender: {our_gender}");

            string database_name = "face_info";
            string collection_name = "face_collection";

            try
            {
                GetStartedDemo().Wait();
                ExecuteSimpleQuery(context, database_name, collection_name);
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

    private async Task GetStartedDemo()
    {
        this.client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);
    }

    private async Task ExecuteSimpleQuery(IDialogContext context, string databaseName, string collectionName)
    {
        // Set some common query options
        FeedOptions queryOptions = new FeedOptions { MaxItemCount = 5 };

        IQueryable<dynamic> picturequery = this.client.CreateDocumentQuery<dynamic>(
            UriFactory.CreateDocumentCollectionUri(databaseName, collectionName),
            "SELECT TOP 5 * FROM c WHERE c.faceAttributes.gender = 'female'",
            queryOptions);

        foreach (dynamic picture in picturequery)
        {
            await context.PostAsync(picture.faceThumbUrl);
        }
    }
}