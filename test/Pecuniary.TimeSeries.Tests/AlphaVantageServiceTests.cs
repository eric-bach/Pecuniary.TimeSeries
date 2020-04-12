using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Pecuniary.TimeSeries.Services;
using Xunit;

namespace Pecuniary.TimeSeries.Tests
{
    public class AlphaVantageServiceTests
    {
        [Fact]
        public async Task ShouldGetSymbol()
        {
            // ARRANGE
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

            var content = $@"
                    {{
                        ""Meta Data"": {{
                                ""1. Information"": ""Daily Prices (open, high, low, close) and Volumes"",
                                ""2. Symbol"": ""{symbol}"",
                                ""3. Last Refreshed"": ""{DateTime.Today:yyyy-MM-dd}"",
                                ""4. Output Size"": ""Compact"",
                                ""5. Time Zone"": ""US/Eastern""
                            }},
                            ""Time Series (Daily)"": {{
                                ""{date}"": {{
                                    ""1. open"": ""{open}"",
                                    ""2. high"": ""{high}"",
                                    ""3. low"": ""{low}"",
                                    ""4. close"": ""{close}"",
                                    ""5. volume"": ""{volume}""
                                }}
                            }}
                    }}";
            var httpClient = MockHttpClient(content);

            var ssmClient = new Mock<AmazonSimpleSystemsManagementClient>();
            ssmClient.Setup(s => s.GetParameterAsync(It.IsAny<GetParameterRequest>(), CancellationToken.None))
                .ReturnsAsync(new GetParameterResponse
                {
                    Parameter = new Parameter
                    {
                        Value = "TestApiKey"
                    }
                });

            var timeSeries = new TimeSeries
            {
                symbol = symbol,
                name = name,
                currency = currency,
                type = type,
                region = region,
                date = date,
                open = decimal.Parse(open),
                low = decimal.Parse(low),
                high = decimal.Parse(high),
                close = decimal.Parse(close),
                volume = long.Parse(volume),
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow
            };
            var today = DateTime.Today;

            // ACT 
            var alphaVantageService = new AlphaVantageService(httpClient, ssmClient.Object);
            var results = await alphaVantageService.GetSymbol(timeSeries, today);

            // ASSERT
            results.Count().Should().Be(1);
            results.First().Symbol.Should().Be(symbol);
            results.First().Name.Should().Be(name);
            results.First().Currency.Should().Be(currency);
            results.First().Type.Should().Be(type);
            results.First().Region.Should().Be(region);
            results.First().Date.Should().Be(date);
            results.First().Open.Should().Be(open);
            results.First().Low.Should().Be(low);
            results.First().High.Should().Be(high);
            results.First().Close.Should().Be(close);
            results.First().Volume.Should().Be(volume);
        }

        [Fact]
        public async Task ShouldNotGetSymbolIfUpToDate()
        {
            // ARRANGE
            const string symbol = "TEST";
            const string name = "Test Symbol";
            const string currency = "CAD";
            const string type = "Equity";
            const string region = "Toronto";
            var date = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
            const string open = "100.00";
            const string low = "100.00";
            const string high = "100.00";
            const string close = "100.00";
            const string volume = "1000000";

            var content = $@"
                    {{
                        ""Meta Data"": {{
                                ""1. Information"": ""Daily Prices (open, high, low, close) and Volumes"",
                                ""2. Symbol"": ""{symbol}"",
                                ""3. Last Refreshed"": ""{DateTime.Today:yyyy-MM-dd}"",
                                ""4. Output Size"": ""Compact"",
                                ""5. Time Zone"": ""US/Eastern""
                            }},
                            ""Time Series (Daily)"": {{
                                ""{date}"": {{
                                    ""1. open"": ""{open}"",
                                    ""2. high"": ""{high}"",
                                    ""3. low"": ""{low}"",
                                    ""4. close"": ""{close}"",
                                    ""5. volume"": ""{volume}""
                                }}
                            }}
                    }}";
            var httpClient = MockHttpClient(content);

            var ssmClient = new Mock<AmazonSimpleSystemsManagementClient>();
            ssmClient.Setup(s => s.GetParameterAsync(It.IsAny<GetParameterRequest>(), CancellationToken.None))
                .ReturnsAsync(new GetParameterResponse
                {
                    Parameter = new Parameter
                    {
                        Value = "TestApiKey"
                    }
                });

            var timeSeries = new TimeSeries
            {
                symbol = symbol,
                name = name,
                currency = currency,
                type = type,
                region = region,
                date = date,
                open = decimal.Parse(open),
                low = decimal.Parse(low),
                high = decimal.Parse(high),
                close = decimal.Parse(close),
                volume = long.Parse(volume),
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow
            };
            var today = DateTime.Today;

            // ACT 
            var alphaVantageService = new AlphaVantageService(httpClient, ssmClient.Object);
            var results = await alphaVantageService.GetSymbol(timeSeries, today);

            // ASSERT
            results.Count().Should().Be(0);
        }

        private static HttpClient MockHttpClient(string content)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(content)
                })
                .Verifiable();
            
            // use real http client with mocked handler here
            return new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };
        }
    }
}
