using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using EricBach.LambdaLogger;
using Newtonsoft.Json;
using Pecuniary.TimeSeries.Models;

namespace Pecuniary.TimeSeries.Services
{
    public class AlphaVantageService
    {
        private readonly HttpClient _httpClient;

        public AlphaVantageService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<IEnumerable<Quotes>> GetSymbol(Models.TimeSeries timeSeries, DateTime date)
        {
            var uri = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={timeSeries.symbol}&outputsize=full&apikey={await GetApiKey()}";

            try
            {
                var responseBody = await _httpClient.GetStringAsync(uri);

                var quotes = ConvertAlphaVantage(timeSeries, responseBody);

                return quotes.Where(q => DateTime.Parse(q.Date) >= date);
            }
            catch (HttpRequestException e)
            {
                Logger.Log($"Message :{e.Message}");
            }

            return new List<Quotes>();
        }

        private static async Task<string> GetApiKey()
        {
            var ssmClient = new AmazonSimpleSystemsManagementClient(RegionEndpoint.USWest2);
            var apiKey = await ssmClient.GetParameterAsync(new GetParameterRequest
            {
                Name = "AlphaVantageApiKey"
            });

            return apiKey.Parameter.Value;
        }

        /// <summary>
        /// Convert the improper AlphaVantageService JSON to a Quote object
        /// </summary>
        /// <param name="timeSeries"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        private static IEnumerable<Quotes> ConvertAlphaVantage(Models.TimeSeries timeSeries, string requestBody)
        {
            var ts = new Regex(@"\""Time\sSeries\s\(Daily\)\"":\s{(.*)}", RegexOptions.Singleline);
            var tsMatches = ts.Matches(requestBody);

            var tsQuotes = new Regex(@"(\d\d\d\d-\d\d-\d\d)\"":\s{(.*?)}", RegexOptions.Singleline);
            var tsQuotesMatches = tsQuotes.Matches(tsMatches[0].Groups[1].Value);

            var quotes = new List<Quotes>();
            try
            {
                for (var i = 0; i < tsQuotesMatches.Count; i++)
                {
                    var quote = JsonConvert.DeserializeObject<Quotes>("{" + tsQuotesMatches[i].Groups[2].Value + "}");
                    quote.Date = tsQuotesMatches[i].Groups[1].Value;
                    quote.Symbol = timeSeries.symbol;
                    quote.Name = timeSeries.name;
                    quote.Region = timeSeries.region;
                    quote.Type = timeSeries.type;
                    quote.Currency = timeSeries.currency;

                    quotes.Add(quote);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }

            return quotes;
        }

    }
}
