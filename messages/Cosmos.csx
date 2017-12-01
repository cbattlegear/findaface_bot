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
        try
        {
            this.client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);
        }
        catch (DocumentClientException de)
        {
            Exception baseException = de.GetBaseException();
            log.Info("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message, baseException.Message);
        }
        catch (Exception e)
        {
            Exception baseException = e.GetBaseException();
            log.Info("Error: {0}, Message: {1}", e.Message, baseException.Message);
        }
    }

    public async Task<List<string>> ExecuteSimpleQuery(string whereclause)
    {
        List<string> thumbnails = new List<string>();
        // Set some common query options
        FeedOptions queryOptions = new FeedOptions { MaxItemCount = 5 };
        try
        {
            IQueryable<dynamic> picturequery = this.client.CreateDocumentQuery<dynamic>(
            UriFactory.CreateDocumentCollectionUri(database_name, collection_name),
            "SELECT TOP 5 * FROM c WHERE " + whereclause,
            queryOptions);
        }
        catch (DocumentClientException de)
        {
            Exception baseException = de.GetBaseException();
            log.Info("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message, baseException.Message);
        }
        catch (Exception e)
        {
            Exception baseException = e.GetBaseException();
            log.Info("Error: {0}, Message: {1}", e.Message, baseException.Message);
        }

        foreach (dynamic picture in picturequery)
        {
            thumbnails.Add(picture.faceThumbUrl);
        }

        return thumbnails;
    }
}