using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDBAccess
{
    class Program
    {
        private static readonly string uri = @"your Cosmos DB Uri";
        private static readonly string key = @"your Cosmos DB Key";
        private static readonly string databaseName = "your Database Name";
        private static readonly string collectionName = "your Container / Collection Name";

        public static string token = @"your LINE access token";
        public static string url = @"https://api.line.me/v2/bot/message/broadcast";

        static void Main(string[] args)
        {
            var items = Task.Run(async () => {
                return await GetHarvestDatas();
            }).GetAwaiter().GetResult();

            foreach(var item in items)
            {
                SendMessage(item);
            }
        }

        static async Task<List<HarvestData>> GetHarvestDatas()
        {
            var client = new DocumentClient(new Uri(uri), key);
            var uriFactory = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);
            var feedOptions = new FeedOptions { MaxItemCount = -1 };

            var harvestDatas = new List<HarvestData>();

            try
            {
                var feed = await client.ReadDocumentFeedAsync(uriFactory, feedOptions);
                
                foreach (Document document in feed)
                {
                    var serializer = new DataContractJsonSerializer(typeof(HarvestData));
                    var ms = new MemoryStream(Encoding.UTF8.GetBytes(document.ToString()));
                    var harvestdata = serializer.ReadObject(ms) as HarvestData;

                    harvestDatas.Add(harvestdata);
                }
                return harvestDatas;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public static void SendMessage(HarvestData harvestData)
        {
            var lineMessage = new LineMessage();

            lineMessage.Message.Add(new Message
            {
                Type = "text",
                Text = "収穫が完了したよ！\n" +
                "収穫日 : " + harvestData.Date + " " + harvestData.Time + "\n" +
                "🌾 : " + harvestData.WheatCount + "\n" +
                "🥔 : " + harvestData.PotatoCount + "\n" +
                "🥕 : " + harvestData.CarrotCount
            });

            var Serializer = new DataContractJsonSerializer(typeof(LineMessage));
            var ms = new MemoryStream();
            Serializer.WriteObject(ms, lineMessage);

            using (WebClient Client = new WebClient())
            {
                Client.Encoding = Encoding.UTF8;
                Client.Headers.Add("Content-Type", "application/json");
                Client.Headers.Add("Authorization", "Bearer " + token);
                Client.UploadString(url, Encoding.UTF8.GetString(ms.ToArray()));
            }
        }
    }

    [DataContract]
    public class HarvestData
    {
        [DataMember(Name = "Date")]
        public string Date { get; set; }

        [DataMember(Name = "Time")]
        public string Time { get; set; }

        [DataMember(Name = "WheatCount")]
        public int WheatCount { get; set; }

        [DataMember(Name = "PotatoCount")]
        public int PotatoCount { get; set; }

        [DataMember(Name = "CarrotCount")]
        public int CarrotCount { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "_rid")]
        public string Rid { get; set; }

        [DataMember(Name = "_self")]
        public string Self { get; set; }

        [DataMember(Name = "_etag")]
        public string Etag { get; set; }

        [DataMember(Name = "_attachments")]
        public string Attachments { get; set; }

        [DataMember(Name = "_ts")]
        public int Ts { get; set; }
    }

    [DataContract]
    public class LineMessage
    {
        [DataMember(Name = "messages")]
        public IList<Message> Message { get; set; } = new List<Message>();
    }

    [DataContract]
    public class Message
    {
        [DataMember(Name = "type")]
        public string Type { get; set; }
        [DataMember(Name = "text")]
        public string Text { get; set; }
    }
}