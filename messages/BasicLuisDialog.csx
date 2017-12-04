#r "Newtonsoft.Json"
#load "Cosmos.csx"

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        await context.PostAsync($"Currently I can find men or women by age, hair color, if they are bald, if they have glasses, are smiling, or the emotion on their face"); //
        context.Wait(MessageReceived);
    }

    // Go to https://luis.ai and create a new intent, then train/publish your luis app.
    // Finally replace "MyIntent" with the name of your newly created intent in the following handler
    [LuisIntent("Find Picture")]
    public async Task FindPicture(IDialogContext context, LuisResult result)
    {
        int numberofpictures = 1;
        EntityRecommendation number;
        if (result.TryFindEntity("builtin.number", out number))
        {
            var our_number = "";
            //JArray mid = (JArray)age.Resolution["values"];

            our_number = number.Resolution["value"].ToString();

            //our_age = mid[0].ToString();
            await context.PostAsync($"You want {our_number} pictures");
            numberofpictures = Convert.ToInt32(our_number);
        }

        string query_build = "";
        bool is_first = true;
        EntityRecommendation gender;
        if(result.TryFindEntity("gender", out gender)) {
            var our_gender = "";
            JArray mid = (JArray)gender.Resolution["values"];

            our_gender = mid[0].ToString();
            //await context.PostAsync($"You sent the Gender: {our_gender}");

            query_build += "c.faceAttributes.gender = '" + our_gender + "'";
            is_first = false;
        }

        EntityRecommendation haircolor;
        if (result.TryFindEntity("haircolor", out haircolor))
        {
            var our_haircolor = "";
            JArray mid = (JArray)haircolor.Resolution["values"];

            our_haircolor = mid[0].ToString();
            //await context.PostAsync($"You sent the Hair Color: {our_haircolor}");
            string query = "c.faceAttributes.hair.hairColor[0].color = '" + our_haircolor + "' and c.faceAttributes.hair.hairColor[0].confidence > 0.9";
            if (is_first)
            {
                query_build += query;
                is_first = false;
            } else
            {
                query = " and " + query;
                query_build += query;
            }
            
        }

        EntityRecommendation emotion;
        if (result.TryFindEntity("emotion", out emotion))
        {
            var our_emotion = "";
            JArray mid = (JArray)emotion.Resolution["values"];

            our_emotion = mid[0].ToString();
            //await context.PostAsync($"You sent the Emotion: {our_emotion}");
            string query = "c.faceAttributes.emotion." + our_emotion + " > 0.9";
            if (is_first)
            {
                query_build += query;
                is_first = false;
            }
            else
            {
                query = " and " + query;
                query_build += query;
            }

        }

        EntityRecommendation age;
        if (result.TryFindEntity("builtin.age", out age))
        {
            var our_age = "";
            //JArray mid = (JArray)age.Resolution["values"];

            our_age = age.Resolution["value"].ToString();

            //our_age = mid[0].ToString();
            //await context.PostAsync($"You sent the Age: {our_age}");
            int int_age = Convert.ToInt32(our_age);
            string query = "c.faceAttributes.age >= " + (int_age - 2).ToString() + " and c.faceAttributes.age <= " + (int_age + 2).ToString();
            if (is_first)
            {
                query_build += query;
                is_first = false;
            }
            else
            {
                query = " and " + query;
                query_build += query;
            }

        }

        EntityRecommendation bald;
        if (result.TryFindEntity("bald", out bald))
        {
            string query = "c.faceAttributes.hair.bald >= 0.9";
            if (is_first)
            {
                query_build += query;
                is_first = false;
            }
            else
            {
                query = " and " + query;
                query_build += query;
            }

        }

        EntityRecommendation glasses;
        if (result.TryFindEntity("glasses", out glasses))
        {
            string query = "c.faceAttributes.glasses <> 'NoGlasses'";
            if (is_first)
            {
                query_build += query;
                is_first = false;
            }
            else
            {
                query = " and " + query;
                query_build += query;
            }

        }

        EntityRecommendation sunglasses;
        if (result.TryFindEntity("sunglasses", out sunglasses))
        {
            string query = "c.faceAttributes.glasses = 'Sunglasses'";
            if (is_first)
            {
                query_build += query;
                is_first = false;
            }
            else
            {
                query = " and " + query;
                query_build += query;
            }

        }

        EntityRecommendation smile;
        if (result.TryFindEntity("smile", out smile))
        {
            string query = "c.faceAttributes.smile >= 0.9";
            if (is_first)
            {
                query_build += query;
                is_first = false;
            }
            else
            {
                query = " and " + query;
                query_build += query;
            }

        }

        try
        {
            Cosmos c = new Cosmos();
            c.OpenConnection().Wait();
            List<string> thumbnails = await c.ExecuteSimpleQuery(query_build, numberofpictures);
            if(thumbnails.Count() == 0)
            {
                await context.PostAsync($"I didn't find any pictures with those attributes, sorry!");
            } else if(thumbnails.Count() == 1)
            {
                await context.PostAsync($"Here's a person I found that looks like what you are asking for.");
                var message = context.MakeMessage();
                foreach (string thumbnail in thumbnails)
                {
                    var attachment = GetHeroCard(thumbnail);
                    message.Attachments.Add(attachment);
                }
                await context.PostAsync(message);
            } else
            {
                await context.PostAsync($"Here's a few people I found that look like what you are asking for.");
                var message = context.MakeMessage();
                foreach (string thumbnail in thumbnails)
                {
                    var attachment = GetHeroCard(thumbnail);
                    message.Attachments.Add(attachment);
                }
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
        context.Wait(MessageReceived);
    }

    private static Microsoft.Bot.Connector.Attachment GetHeroCard(string image_url)
    {
        var heroCard = new HeroCard
        {
            Images = new List<CardImage> { new CardImage(image_url) },
            Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "View Full Picture", value: image_url) }
        };

        return heroCard.ToAttachment();
    }

    public static string StripIncompatableQuotes(string s)
    {
        if (!string.IsNullOrEmpty(s))
            return s.Replace('\u2018', '\'').Replace('\u2019', '\'').Replace('\u201c', '\"').Replace('\u201d', '\"');
        else
            return s;
    }
    
}