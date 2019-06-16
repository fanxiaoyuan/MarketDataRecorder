using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;

namespace MarketDataRecorder.DataModel
{
    public class StreamingData : IMarketData
    {
        [JsonProperty("microtimestamp")]
        [Name("microtimestamp")]
        public long MicroTimeStamp { get; set; }

        [JsonProperty("amount")]
        [Name("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("buy_order_id")]
        [Name("buy_order_id")]
        public long BuyOrderId { get; set; }

        [JsonProperty("sell_order_id")]
        [Name("sell_order_id")]
        public long SellOrderId { get; set; }

        [JsonProperty("amount_str")]
        [Name("amount_str")]
        public string AmountString { get; set; }

        [JsonProperty("price_str")]
        [Name("price_str")]
        public string PriceString { get; set; }

        [JsonProperty("price")]
        [Name("price")]
        public string Price { get; set; }

        [JsonProperty("type")]
        [Name("type")]
        public long Type { get; set; }

        [JsonProperty("id")]
        [Name("id")]
        public long Id { get; set; }

        [JsonIgnore]
        [Name("currency_pair")]
        public string CurrencyPair { get; set; }
    }
}
