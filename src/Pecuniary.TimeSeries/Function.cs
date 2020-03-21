using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using EricBach.LambdaLogger;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Pecuniary.TimeSeries
{
    public class Function
    {
        private readonly AmazonDynamoDBClient _dynamoDbClient;
        private readonly DynamoDBContext _dynamoDbContext;
        // TODO Environment variable
        private static string tableName = "TimeSeries-5xjfz6mpa5g2rgwc47wfyqzjja-dev";

        public Function()
        {
            _dynamoDbClient = new AmazonDynamoDBClient(RegionEndpoint.USWest2);
            _dynamoDbContext = new DynamoDBContext(_dynamoDbClient);
        }

        public async Task FunctionHandler(ILambdaContext context)
        {
            var timeSeries = await GetAsync<TimeSeries>();

            foreach (var t in timeSeries)
            {
                Logger.Log(t.Symbol);
            }
        }

        private async Task<ICollection<T>> GetAsync<T>()
        {
            Logger.Log($"Scanning DynamoDB {tableName} for all TimeSeries");
        
            ICollection<T> results = new List<T>();
            try
            {
                var docs = await _dynamoDbClient.ScanAsync(new ScanRequest(tableName));
                
                Logger.Log($"Found items: {docs.Items.Count}");

                foreach (var t in docs.Items.Select(i => DocumentToClass<T>(_dynamoDbContext, i)))
                {
                    results.Add(t);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }

            return results;
        }

        private static T DocumentToClass<T>(DynamoDBContext context, Dictionary<string, AttributeValue> obj)
        {
            var doc = Document.FromAttributeMap(obj);
            return context.FromDocument<T>(doc);
        }
    }

    // TODO Move to class
    public class TimeSeries
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("close")]
        public decimal Close { get; set; }
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; }
        [JsonProperty("date")]
        public string Date { get; set; }
        [JsonProperty("high")]
        public decimal High { get; set; }
        [JsonProperty("low")]
        public decimal Low { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("open")]
        public decimal Open { get; set; }
        [JsonProperty("region")]
        public string Region { get; set; }
        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }
        [JsonProperty("volume")]
        public long Volume { get; set; }
    }
}
