using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using EricBach.LambdaLogger;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Pecuniary.TimeSeries
{
    public class Function
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly AmazonDynamoDBClient _dynamoDbClient;
        private static string tableName = "TimeSeries-5xjfz6mpa5g2rgwc47wfyqzjja-dev";

        //public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        public async Task<ICollection<Dictionary<string, AttributeValue>>> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            //var location = await GetCallingIP();
            //var body = new Dictionary<string, string>
            //{
            //    { "message", "hello world" },
            //    { "location", location }
            //};

            //return new APIGatewayProxyResponse
            //{
            //    Body = JsonConvert.SerializeObject(body),
            //    StatusCode = 200,
            //    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            //};

            return await GetAsync();
        }

        private async Task<ICollection<Dictionary<string, AttributeValue>>> GetAsync()
        {
            Logger.Log($"Scanning DynamoDB {tableName} for all TimeSeries");
            
            var _dynamoDbClient = new AmazonDynamoDBClient(RegionEndpoint.USWest2);
            List<Dictionary<string, AttributeValue>> items = new List<Dictionary<string, AttributeValue>>();
            Logger.Log(_dynamoDbClient.ToString());
            try
            {
                var results =  await _dynamoDbClient.ScanAsync(new ScanRequest(tableName));
                Logger.Log($"Found items: {results.Items.Count}");
                items = results.Items;
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }

            //var context = new DynamoDBContext(_dynamoDbClient);
            //var conditions = new List<ScanCondition>();
            //var docs = await context.ScanAsync<TimeSeries>(conditions).GetRemainingAsync();

            return items;
        }

        private static async Task<string> GetCallingIP()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "AWS Lambda .Net Client");

            var msg = await client.GetStringAsync("http://checkip.amazonaws.com/").ConfigureAwait(continueOnCapturedContext: false);

            return msg.Replace("\n", "");
        }
    }

    public class TimeSeries
    {
        public Guid Id { get; set; }
        public decimal Close { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Currency { get; set; }
        public string Date { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public string Name { get; set; }
        public decimal Open { get; set; }
        public string Region { get; set; }
        public string Symbol { get; set; }
        public string Type { get; set; }
        public DateTime UpdatedAt { get; set; }
        public long Volume { get; set; }
    }
}
