using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using EricBach.LambdaLogger;
using Microsoft.Extensions.DependencyInjection;
using Pecuniary.TimeSeries.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Pecuniary.TimeSeries
{
    public class Function
    {
        public virtual AlphaVantageService AlphaVantageService { get; }
        public virtual IDynamoDbService DynamoDbService { get; }

        public Function()
        {
            var serviceProvider = ConfigureServices();

            AlphaVantageService = serviceProvider.GetService<AlphaVantageService>();
            DynamoDbService = serviceProvider.GetService<IDynamoDbService>();
        }

        /// <summary>
        /// Used for unit testing
        /// </summary>
        /// <param name="dynamoDbService"></param>
        /// <param name="alphaVantageService"></param>
        public Function(IDynamoDbService dynamoDbService, AlphaVantageService alphaVantageService)
        {
            AlphaVantageService = alphaVantageService;
            DynamoDbService = dynamoDbService;
        }

        private static ServiceProvider ConfigureServices()
        {
            var serviceProvider = new ServiceCollection()
                .AddScoped<AlphaVantageService>()
                .AddTransient<IDynamoDbService, DynamoDbService>()
                .BuildServiceProvider();

            return serviceProvider;
        }

        public async Task FunctionHandler()
        {
            // Get all the unique symbols
            var docs = await DynamoDbService.GetAllAsync<TimeSeries>();
            var timeSeries = FilterTimeSeries(docs);

            // Get the quotes for the unique symbols
            foreach (var t in timeSeries)
            {
                var symbol = await AlphaVantageService.GetSymbol(t, DateTime.Parse(t.date));

                // Save quotes to DynamoDB
                await DynamoDbService.SaveToDynamoAsync(symbol);
            }
        }

        /// <summary>
        /// Returns a sorted collection of unique time series symbols. If more than one symbol is found return the one with the most recent date
        /// </summary>
        /// <param name="timeSeries"></param>
        /// <returns></returns>
        private static IEnumerable<TimeSeries> FilterTimeSeries(IEnumerable<TimeSeries> timeSeries)
        {
            var groupedTimeSeries = timeSeries.GroupBy(t => t.symbol).ToList();

            var symbols = new List<TimeSeries>();
            for (var i = 0; i < groupedTimeSeries.Count; i++)
            {
                // Take the time series with the most recent date
                var symbol = groupedTimeSeries.ToList()[i].ToList().OrderByDescending(t => t.date).First();

                symbols.Add(symbol);
            }

            Logger.Log($"TEMP: Returning {symbols.Count} symbols");
            return symbols.OrderBy(t => t.symbol).ThenBy(t => t.date);
        }
    }
}
