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

        public async Task<ICollection<TimeSeries>> FunctionHandler(ILambdaContext context)
        {
            return await GetAsync<TimeSeries>();
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
        public Guid id { get; set; }
        public decimal close { get; set; }
        public DateTime createdAt { get; set; }
        public string currency { get; set; }
        public string date { get; set; }
        public decimal high { get; set; }
        public decimal low { get; set; }
        public string name { get; set; }
        public decimal open { get; set; }
        public string region { get; set; }
        public string symbol { get; set; }
        public string type { get; set; }
        public DateTime updatedAt { get; set; }
        public long volume { get; set; }
    }
}
