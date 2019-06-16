using System;
using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;

namespace MarketDataRecorder.DataModel
{
    public class SnapshotData : IMarketData
    {
        [JsonProperty("last")]
        [Name("last")]
        public decimal Last { get; set; }

        [JsonProperty("high")]
        [Name("high")]
        public decimal High { get; set; }

        [JsonProperty("low")]
        [Name("low")]
        public decimal Low { get; set; }

        [JsonProperty("vwap")]
        [Name("vwap")]
        public decimal Vwap { get; set; }

        [JsonProperty("volume")]
        [Name("volume")]
        public decimal Volume { get; set; }

        [JsonProperty("bid")]
        [Name("bid")]
        public decimal Bid { get; set; }

        [JsonProperty("ask")]
        [Name("ask")]
        public decimal Ask { get; set; }

        [JsonProperty("timestamp")]
        [Name("timestamp")]
        public long UnixTimestamp { get; set; }

        [JsonIgnore]
        [Name("currency_pair")]
        public string CurrencyPair { get; set; }

        [JsonIgnore]
        [Name("getTimestamp")]
        [Format("yyyyMMdd hh:mm:ss.fff")]
        public DateTime Timestamp => DateTimeOffset.FromUnixTimeSeconds(UnixTimestamp).UtcDateTime;
    }
}
