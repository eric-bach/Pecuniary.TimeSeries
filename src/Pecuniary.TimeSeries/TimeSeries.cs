using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Pecuniary.TimeSeries
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class TimeSeries
    {
        public Guid id { get; set; }
        public decimal close { get; set; }
        public DateTime createdAt { get; set; }
        public string currency { get; set; }
        public string date { get; set; }
        public decimal high { get; set; }
        public decimal low { get; set; }
        public string name { get; set; }
        public decimal open { get; set; }
        public string region { get; set; }
        public string symbol { get; set; }
        public string type { get; set; }
        public DateTime updatedAt { get; set; }
        public long volume { get; set; }
    }

    public class Quotes
    {
        public string Date { get; set; }
        [JsonProperty("1. open")]
        public string Open { get; set; }
        [JsonProperty("2. high")]
        public string High { get; set; }
    }

}