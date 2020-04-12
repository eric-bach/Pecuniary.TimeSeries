using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using FluentAssertions;
using Moq;
using Pecuniary.TimeSeries.Services;
using Xunit;

namespace Pecuniary.TimeSeries.Tests
{
    public class DynamoDbServiceTests
    {
        public DynamoDbServiceTests()
        {
            Environment.SetEnvironmentVariable("TableName", "TestTable");
        }

        [Fact]
        public async Task ShouldConvertDynamoDbDocumentToTimeSeries()
        {
            // ARRANGE

            var amazonDynamoDbClient = new Mock<AmazonDynamoDBClient>();
            var dynamoDbContext = new Mock<DynamoDBContext>(amazonDynamoDbClient.Object);

            var id = new Guid().ToString();
            const string symbol = "TEST";
            const string name = "Test Symbol";
            const string currency = "CAD";
            const string type = "Equity";
            const string region = "Toronto";
            var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
            const string open = "100.00";
            const string low = "100.00";
            const string high = "100.00";
            const string close = "100.00";
            const string volume = "1000000";
            var createdAt = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
            var updatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);

            amazonDynamoDbClient.Setup(d => d.ScanAsync(It.IsAny<ScanRequest>(), CancellationToken.None))
                .ReturnsAsync(new ScanResponse
                {
                    Items = new List<Dictionary<string, AttributeValue>>
                    {
                        new Dictionary<string, AttributeValue>
                        {
                            {
                                "__typename", new AttributeValue
                                {
                                    S = nameof(TimeSeries)
                                }
                            },
                            {
                                "id", new AttributeValue
                                {
                                    S = id
                                }
                            },
                            {
                                "symbol", new AttributeValue
                                {
                                    S = symbol
                                }
                            },
                            {
                                "name", new AttributeValue
                                {
                                    S = name
                                }
                            },
                            {
                                "currency", new AttributeValue
                                {
                                    S = currency
                                }
                            },
                            {
                                "type", new AttributeValue
                                {
                                    S = type
                                }
                            },
                            {
                                "region", new AttributeValue
                                {
                                    S = region
                                }
                            },
                            {
                                "date", new AttributeValue
                                {
                                    S = date
                                }
                            },
                            {
                                "open", new AttributeValue
                                {
                                    S = open
                                }
                            },
                            {
                                "low", new AttributeValue
                                {
                                    S = low
                                }
                            },
                            {
                                "high", new AttributeValue
                                {
                                    S = high
                                }
                            },
                            {
                                "close", new AttributeValue
                                {
                                    S = close
                                }
                            },
                            {
                                "volume", new AttributeValue
                                {
                                    S = volume
                                }
                            },
                            {
                                "createdAt", new AttributeValue
                                {
                                    S = createdAt
                                }
                            },
                            {
                                "updatedAt", new AttributeValue
                                {
                                    S = updatedAt
                                }
                            }
                        }
                    }
                });
            var dynamoDbService = new DynamoDbService(amazonDynamoDbClient.Object, dynamoDbContext.Object);

            // ACT

            var results = await dynamoDbService.GetAllAsync<TimeSeries>();

            // ASSERT

            results.Count.Should().Be(1);
            results.First().id.Should().Be(id);
            results.First().symbol.Should().Be(symbol);
            results.First().name.Should().Be(name);
            results.First().currency.Should().Be(currency);
            results.First().type.Should().Be(type);
            results.First().region.Should().Be(region);
            results.First().date.Should().Be(date);
            results.First().open.Should().Be(decimal.Parse(open));
            results.First().low.Should().Be(decimal.Parse(low));
            results.First().high.Should().Be(decimal.Parse(high));
            results.First().close.Should().Be(decimal.Parse(close));
            results.First().volume.Should().Be(long.Parse(volume));
            results.First().createdAt.ToString("yyyy-MM-dd").Should().Be(DateTime.Parse(createdAt).ToString("yyyy-MM-dd"));
            results.First().updatedAt.ToString("yyyy-MM-dd").Should().Be(DateTime.Parse(createdAt).ToString("yyyy-MM-dd"));
        }

        [Fact]
        public async Task ShouldHandleException()
        {
            // ARRANGE

            var amazonDynamoDbClient = new Mock<AmazonDynamoDBClient>();
            var dynamoDbContext = new Mock<DynamoDBContext>(amazonDynamoDbClient.Object);

            amazonDynamoDbClient.Setup(d => d.ScanAsync(It.IsAny<ScanRequest>(), CancellationToken.None))
                .ThrowsAsync(new Exception());
            var dynamoDbService = new DynamoDbService(amazonDynamoDbClient.Object, dynamoDbContext.Object);

            // ACT

            var results = await dynamoDbService.GetAllAsync<TimeSeries>();

            // ASSERT

            results.Count.Should().Be(0);
        }
    }
}