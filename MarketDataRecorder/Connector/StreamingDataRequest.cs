using Newtonsoft.Json;

namespace MarketDataRecorder.Connector
{
    public class StreamingDataRequest
    {
        [JsonProperty("event")]
        public string EventName { get; set; }

        [JsonProperty("data")]
        public RequestData Data { get; set; }
    }

    public class RequestData
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }
    }
}
