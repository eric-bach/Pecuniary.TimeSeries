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
        private AlphaVantageService AlphaVantageService { get; }
        public IDynamoDbService DynamoDbService { get; set; }

        private static string _tableName;

        public Function()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            _tableName = Environment.GetEnvironmentVariable("TableName") ?? "TimeSeries-5xjfz6mpa5g2rgwc47wfyqzjja-dev";

            AlphaVantageService = serviceProvider.GetService<AlphaVantageService>();
            DynamoDbService = serviceProvider.GetService<IDynamoDbService>();
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<AlphaVantageService>();
            serviceCollection.AddTransient<IDynamoDbService>(t => new DynamoDbService(_tableName));
        }

        public async Task FunctionHandler()
        {
            // Get all the unique symbols
            var docs = await DynamoDbService.GetAllAsync<Models.TimeSeries>(_tableName);
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
        private static IEnumerable<Models.TimeSeries> FilterTimeSeries(IEnumerable<Models.TimeSeries> timeSeries)
        {
            var groupedTimeSeries = timeSeries.GroupBy(t => t.symbol).ToList();

            var symbols = new List<Models.TimeSeries>();
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
