#load "Picture.csx"

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

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
        await context.PostAsync($"Running query");
        IQueryable <Picture> picturequery = this.client.CreateDocumentQuery<Picture>(
        UriFactory.CreateDocumentCollectionUri(database_name, collection_name),
        "SELECT TOP 200 c.faceId, c.faceUrl, c.faceThumbUrl FROM c WHERE " + whereclause,
        queryOptions);

        var picture_list = picturequery.Select(s => new { s.faceId, s.faceUrl, s.faceThumbUrl }).ToList();
        await context.PostAsync($"Converted to list with {picture_list.Count()} pictures");
        Random rand = new Random();
        // 1st Picture
        int rand_pic1 = rand.Next(0, picture_list.Count() - 1);
        thumbnails.Add(picture_list[rand_pic1].faceUrl);
        await context.PostAsync($"Added Picture number {rand_pic1}");
        // 2nd Picture
        int rand_pic2 = rand.Next(0, picture_list.Count() - 1);
        thumbnails.Add(picture_list[rand_pic2].faceUrl);
        await context.PostAsync($"Added Picture number {rand_pic2}");
        // 3rd Picture
        int rand_pic3 = rand.Next(0, picture_list.Count() - 1);
        thumbnails.Add(picture_list[rand_pic3].faceUrl);
        await context.PostAsync($"Added Picture number {rand_pic3}");

        return thumbnails;
    }
}