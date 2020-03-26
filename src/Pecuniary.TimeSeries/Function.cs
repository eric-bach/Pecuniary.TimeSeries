using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Pecuniary.TimeSeries
{
    public class Function
    {
        private AlphaVantageService _alphaVantageService { get; set; }
        public IDynamoDbService _dynamoDbService { get; set; }

        // TODO Environment variable
        private static readonly string _tableName = "TimeSeries-5xjfz6mpa5g2rgwc47wfyqzjja-dev";

        public Function()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            _alphaVantageService = serviceProvider.GetService<AlphaVantageService>();
            _dynamoDbService = serviceProvider.GetService<IDynamoDbService>();
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<AlphaVantageService>();
            serviceCollection.AddTransient<IDynamoDbService>(t => new DynamoDbService(_tableName));
        }

        public async Task FunctionHandler()
        {
            // Get all the unique symbols
            var docs = await _dynamoDbService.GetAllAsync<TimeSeries>(_tableName);
            var timeSeries = FilterTimeSeries(docs);

            // Get the quotes for the unique symbols
            foreach (var t in timeSeries)
            {
                var symbol = await _alphaVantageService.GetSymbol(t, DateTime.Parse(t.date));

                // Save quotes to DynamoDB
                await _dynamoDbService.SaveToDynamoAsync(symbol);
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

            return symbols.OrderBy(t => t.symbol).ThenBy(t => t.date);
        }
    }
}
