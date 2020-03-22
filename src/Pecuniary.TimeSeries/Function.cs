using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using EricBach.LambdaLogger;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Pecuniary.TimeSeries
{
    public class Function
    {
        private readonly AmazonDynamoDBClient _dynamoDbClient;
        private readonly DynamoDBContext _dynamoDbContext;
        private readonly HttpClient _httpClient;
        // TODO Environment variable
        private static readonly string tableName = "TimeSeries-5xjfz6mpa5g2rgwc47wfyqzjja-dev";


        public Function()
        {
            _dynamoDbClient = new AmazonDynamoDBClient(RegionEndpoint.USWest2);
            _dynamoDbContext = new DynamoDBContext(_dynamoDbClient);
            _httpClient = new HttpClient();
        }

        public async Task FunctionHandler(ILambdaContext context)
        {
            var timeSeries = await GetAsync<TimeSeries>();
            
            foreach (var t in timeSeries.OrderBy(t => t.symbol).ThenBy(t => t.date))
            {
                await GetSymbol(t.symbol, DateTime.Parse(t.date));
            }
        }

        private async Task GetSymbol(string symbol, DateTime date)
        {
            AmazonSimpleSystemsManagementClient ssmClient = new AmazonSimpleSystemsManagementClient(RegionEndpoint.USWest2);
            var apiKey = await ssmClient.GetParameterAsync(new GetParameterRequest
            {
                Name = "AlphaVantageApiKey"
            });

            var uri = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={apiKey}";
            //var uri = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&outputsize=full&apikey={apiKey}";

            try
            {
                var responseBody = await _httpClient.GetStringAsync(uri);

                var quotes = ConvertAlphaVantage(responseBody);

                Logger.Log(responseBody);
            }
            catch (HttpRequestException e)
            {
                Logger.Log($"Message :{e.Message}");
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

        private static List<Quotes> ConvertAlphaVantage(string requestBody)
        {
            var timeSeries = new Regex(@"\""Time\sSeries\s\(Daily\)\"":\s{(.*)}", RegexOptions.Singleline);
            var timeSeriesMatches = timeSeries.Matches(requestBody);

            var timeSeriesQuotes = new Regex(@"(\d\d\d\d-\d\d-\d\d)\"":\s{(.*?)}", RegexOptions.Singleline);
            var timeSeriesQuotesMatches = timeSeriesQuotes.Matches(timeSeriesMatches[0].Groups[1].Value);

            var quotes = new List<Quotes>();
            try
            {
                for (var i = 0; i < timeSeriesQuotesMatches.Count; i++)
                {
                    var quote = JsonConvert.DeserializeObject<Quotes>("{" + timeSeriesQuotesMatches[i].Groups[2].Value + "}");
                    quote.Date = timeSeriesQuotesMatches[i].Groups[1].Value;

                    quotes.Add(quote);
                }
            }
            catch (Exception e)
            {
            }

            return quotes;
        }
    }
}
