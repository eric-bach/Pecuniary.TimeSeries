using Newtonsoft.Json;

namespace Pecuniary.TimeSeries.Models
{
    public class Quotes
    {
        public string Date { get; set; }
        public string Symbol { get; set; }
        public string Currency { get; set; }
        public string Name { get; set; }
        public string Region { get; set; }
        public string Type { get; set; }
        [JsonProperty("1. open")] public string Open { get; set; }
        [JsonProperty("2. high")] public string High { get; set; }
        [JsonProperty("3. low")] public string Low { get; set; }
        [JsonProperty("4. close")] public string Close { get; set; }
        [JsonProperty("5. volume")] public string Volume { get; set; }
    }
}