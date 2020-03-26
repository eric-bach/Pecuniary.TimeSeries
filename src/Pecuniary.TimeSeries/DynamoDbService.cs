using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using EricBach.LambdaLogger;

namespace Pecuniary.TimeSeries
{
    public interface IDynamoDbService
    {
        Task<ICollection<T>> GetAllAsync<T>(string tableName);
        Task SaveToDynamoAsync(IEnumerable<Quotes> symbols);
    }

    public class DynamoDbService : IDynamoDbService
    {
        private readonly AmazonDynamoDBClient _dynamoDbClient;
        private readonly DynamoDBContext _dynamoDbContext;
        private readonly string _tableName;

        public DynamoDbService(string tableName)
        {
            _dynamoDbClient = new AmazonDynamoDBClient(RegionEndpoint.USWest2);
            _dynamoDbContext = new DynamoDBContext(_dynamoDbClient);

            _tableName = tableName;
        }

        /// <summary>
        /// Returns all documents from DynamoDB table
        /// </summary>
        /// <typeparam name="T">object</typeparam>
        /// <returns></returns>
        public async Task<ICollection<T>> GetAllAsync<T>(string tableName)
        {
            Logger.Log($"Scanning DynamoDB {tableName} for all TimeSeries");

            ICollection<T> results = new List<T>();
            try
            {
                var docs = await _dynamoDbClient.ScanAsync(new ScanRequest(tableName));

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

        public async Task SaveToDynamoAsync(IEnumerable<Quotes> symbols)
        {
            foreach (var s in symbols)
            {
                try
                {
                    var table = Table.LoadTable(_dynamoDbClient, _tableName);

                    var record = new Document
                    {
                        ["close"] = s.Close,
                        ["createdAt"] = DateTime.UtcNow.ToString("O"),
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
                        ["updatedAt"] = DateTime.UtcNow.ToString("O"),
                        ["volume"] = s.Volume
                    };

                    await table.PutItemAsync(record);
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
