using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Moq;
using Pecuniary.TimeSeries.Services;
using Xunit;

namespace Pecuniary.TimeSeries.Tests
{
    public class FunctionTest
    {
        [Fact(Skip="Integration Test")]
        public async Task IntegrationTest()
        {
            Environment.SetEnvironmentVariable("TableName", "TimeSeries-5xjfz6mpa5g2rgwc47wfyqzjja-dev");
            
            var function = new Function();

            await function.FunctionHandler();
        }

        [Fact]
        public async Task ShouldSaveQuote()
        {
            // ARRANGE

            var mockDynamoDbService = new Mock<DynamoDbService>();
            var mockAlphaVantageService = new Mock<AlphaVantageService>();

            mockDynamoDbService.Setup(m => m.GetAllAsync<TimeSeries>())
                .ReturnsAsync(new List<TimeSeries>
                {
                    new TimeSeries
                    {
                        id = new Guid(),
                        close = 100m,
                        createdAt = DateTime.Now,
                        currency = "CAD",
                        date = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                        high = 100m,
                        low = 100m,
                        name = "TEST",
                        open = 100m,
                        region = "Toronto",
                        symbol = "TEST",
                        type = "Equity",
                        updatedAt = DateTime.Now,
                        volume = 100
                    }
                });
            mockDynamoDbService.Setup(d => d.SaveToDynamoAsync(It.IsAny<IEnumerable<Quotes>>())).Returns(Task.CompletedTask);
            mockAlphaVantageService.Setup(a => a.GetSymbol(It.IsAny<TimeSeries>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<Quotes>
                {
                    new Quotes
                    {
                        Date = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                        Symbol = "TEST",
                        Currency = "CAD",
                        Name = "TEST",
                        Region = "Toronto",
                        Type = "Equity",
                        Open = "100",
                        High = "100",
                        Low = "100",
                        Close = "100",
                        Volume= "100"
                    }
                });
            var function = new Function(mockDynamoDbService.Object, mockAlphaVantageService.Object);

            // ACT

            await function.FunctionHandler();

            // ASSERT

        }
    }
}