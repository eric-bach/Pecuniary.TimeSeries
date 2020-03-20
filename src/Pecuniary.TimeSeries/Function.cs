using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
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


        public Function()
        {
            _dynamoDbClient = new AmazonDynamoDBClient(RegionEndpoint.USWest2);
        }

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
            Logger.Log($"Scanning DynamoDB for all TimeSeries");

            var context = new DynamoDBContext(_dynamoDbClient);

            var results = await _dynamoDbClient.ScanAsync(new ScanRequest(tableName));
            

            //var conditions = new List<ScanCondition>();
            //var docs = await context.ScanAsync<TimeSeries>(conditions).GetRemainingAsync();

            Logger.Log($"Found items: {results.Items.Count}");

            return results.Items;
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
