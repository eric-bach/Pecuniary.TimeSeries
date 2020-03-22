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
            // Get all the unique symbols
            var timeSeries = FilterTimeSeries(await GetAsync<TimeSeries>());

            // Get the quotes for the unique symbols
            var symbols = new List<Quotes>();
            foreach (var t in timeSeries.OrderBy(t => t.symbol).ThenBy(t => t.date))
            {
                var symbol = await GetSymbol(t.symbol, DateTime.Parse(t.date));

                symbols.AddRange(symbol);
            }

            // TODO Write back to DynamoDB
        }

        private async Task<IEnumerable<Quotes>> GetSymbol(string symbol, DateTime date)
        {
            var uri = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={await GetApiKey()}";
            //var uri = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&outputsize=full&apikey={await GetApiKey()}";

            try
            {
                var responseBody = await _httpClient.GetStringAsync(uri);

                var quotes = ConvertAlphaVantage(symbol, responseBody);

                return quotes.Where(q => DateTime.Parse(q.Date) >= date);
            }
            catch (HttpRequestException e)
            {
                Logger.Log($"Message :{e.Message}");
            }

            return new List<Quotes>();
        }

        /// <summary>
        /// Returns unique time series symbols
        /// </summary>
        /// <param name="timeSeries"></param>
        /// <returns></returns>
        private static IEnumerable<TimeSeries> FilterTimeSeries(IEnumerable<TimeSeries> timeSeries)
        {
            var groupedTimeSeries = timeSeries.GroupBy(t => t.symbol).ToList();
            var symbols = new List<TimeSeries>();

            for (var i = 0; i < groupedTimeSeries.Count; i++)
            {
                var symbol = groupedTimeSeries.ToList()[i].ToList().OrderByDescending(t => t.date).First();

                symbols.Add(symbol);
            }

            return symbols;
        }

        private async Task<string> GetApiKey()
        {
            var ssmClient = new AmazonSimpleSystemsManagementClient(RegionEndpoint.USWest2);
            var apiKey = await ssmClient.GetParameterAsync(new GetParameterRequest
            {
                Name = "AlphaVantageApiKey"
            });

            return apiKey.Parameter.Value;
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

        /// <summary>
        /// Convert the improper AlphaVantage JSON to a Quote object
        /// </summary>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        private static IEnumerable<Quotes> ConvertAlphaVantage(string symbol, string requestBody)
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
                    quote.Symbol = symbol;

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
