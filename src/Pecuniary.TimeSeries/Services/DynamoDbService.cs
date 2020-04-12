using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using EricBach.LambdaLogger;

namespace Pecuniary.TimeSeries.Services
{
    public class DynamoDbService : IDynamoDbService
    {
        private readonly AmazonDynamoDBClient _dynamoDbClient;
        private readonly DynamoDBContext _dynamoDbContext;

        private readonly string _tableName = Environment.GetEnvironmentVariable("TableName");

        public DynamoDbService()
        {
            _dynamoDbClient = new AmazonDynamoDBClient(RegionEndpoint.USWest2);
            _dynamoDbContext = new DynamoDBContext(_dynamoDbClient);
        }

        public DynamoDbService(AmazonDynamoDBClient amazonDynamoDbClient, DynamoDBContext dynamoDbContext)
        {
            _dynamoDbClient = amazonDynamoDbClient;
            _dynamoDbContext = dynamoDbContext;
        }

        /// <summary>
        /// Returns all documents from DynamoDB table
        /// </summary>
        /// <typeparam name="T">object</typeparam>
        /// <returns></returns>
        public virtual async Task<ICollection<T>> GetAllAsync<T>()
        {
            Logger.Log($"Scanning DynamoDB {_tableName} for all TimeSeries");

            ICollection<T> results = new List<T>();
            try
            {
                var docs = await _dynamoDbClient.ScanAsync(new ScanRequest(_tableName));

                Logger.Log($"Found items: {docs.Items.Count}");

                foreach (var t in docs.Items)
                {
                    // Convert DynamoDB document to C# object
                    var doc = Document.FromAttributeMap(t);
                    var obj = _dynamoDbContext.FromDocument<T>(doc);

                    results.Add(obj);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }

            return results;
        }

        [ExcludeFromCodeCoverage]
        public virtual async Task SaveToDynamoAsync(IEnumerable<Quotes> symbols)
        {
            foreach (var s in symbols)
            {
                try
                {
                    var table = Table.LoadTable(_dynamoDbClient, _tableName);

                    var record = new Document
                    {
                        ["close"] = s.Close,
                        ["createdAt"] = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture),
                        ["currency"] = s.Currency,
                        ["date"] = s.Date,
                        ["high"] = s.High,
                        ["id"] = Guid.NewGuid(),
                        ["low"] = s.Low,
                        ["name"] = s.Name,
                        ["open"] = s.Open,
                        ["region"] = s.Region,
                        ["symbol"] = s.Symbol,
                        ["type"] = s.Type,
                        ["updatedAt"] = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture),
                        ["volume"] = s.Volume
                    };

                    await table.PutItemAsync(record);
                }
                catch (Exception e)
                {
                    Logger.Log(e.Message);
                }
            }
        }
    }
}
