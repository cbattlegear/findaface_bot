using System;
using System.Threading.Tasks;
using System.Collections.Generic;

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

    public async Task OpenConnection()
    {
        this.client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);
        
    }

    public async Task<List<string>> ExecuteSimpleQuery(string whereclause, IDialogContext context)
    {
        List<string> thumbnails = new List<string>();
        // Set some common query options
        FeedOptions queryOptions = new FeedOptions { MaxItemCount = 5 };

        IQueryable <dynamic> picturequery = this.client.CreateDocumentQuery<dynamic>(
        UriFactory.CreateDocumentCollectionUri(database_name, collection_name),
        "SELECT TOP 3 * FROM c WHERE " + whereclause,
        queryOptions);

        foreach (dynamic picture in picturequery)
        {
            thumbnails.Add(picture.faceUrl);
        }

        return thumbnails;
    }
}