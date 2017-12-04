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

    public async Task<List<string>> ExecuteSimpleQuery(string whereclause, int numberofpictures = 0)
    {
        List<Picture> thumbnails = new List<Picture>();
        // Set some common query options
        FeedOptions queryOptions = new FeedOptions { MaxItemCount = 5 };
        string query = "";
        if(whereclause == "")
        {
            query = "SELECT TOP 200 * FROM c";
        } else
        {
            query = "SELECT TOP 200 * FROM c WHERE " + whereclause;
        }

        IQueryable<dynamic> picturequery = this.client.CreateDocumentQuery<dynamic>(
        UriFactory.CreateDocumentCollectionUri(database_name, collection_name),
        query,
        queryOptions);

        List<Picture> all_pictures = new List<Picture>();

        foreach (dynamic picture in picturequery)
        {
            all_pictures.Add(new Picture(picture.faceId, picture.faceUrl, picture.faceThumbUrl));
        }

        if(all_pictures.Count() > 0)
        {
            Random rand = new Random();

            for (int i = 0; i < numberofpictures; i++)
            {
                thumbnails.Add(all_pictures[rand.Next(0, all_pictures.Count() - 1)]);
            }
        }

        return thumbnails;
    }
}