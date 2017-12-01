using System;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

// For more information about this template visit http://aka.ms/azurebots-csharp-luis

public class Cosmos
{
    private string EndpointUrl = Utils.GetAppSetting("CosmosEndpoint");
    private string PrimaryKey = Utils.GetAppSetting("CosmosPrimaryKey");
    private string database_name = "face_info";
    private string collection_name = "face_collection";

    private DocumentClient client;
    public Cosmos()
    {
    }

    private async Task OpenConnection()
    {
        this.client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);
    }

    private async Task ExecuteSimpleQuery(IDialogContext context, string whereclause)
    {
        // Set some common query options
        FeedOptions queryOptions = new FeedOptions { MaxItemCount = 5 };

        IQueryable<dynamic> picturequery = this.client.CreateDocumentQuery<dynamic>(
            UriFactory.CreateDocumentCollectionUri(database_name, collection_name),
            "SELECT TOP 5 * FROM c WHERE " + whereclause,
            queryOptions);

        foreach (dynamic picture in picturequery)
        {
            await context.PostAsync(picture.faceThumbUrl);
        }
    }
}