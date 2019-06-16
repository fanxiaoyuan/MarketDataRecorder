using System;
using System.Net.Http;
using System.Threading.Tasks;
using log4net;
using MarketDataRecorder.DataModel;
using Newtonsoft.Json;

namespace MarketDataRecorder.Connector
{
    public class HttpConnector<T> : IConnector<T> where T : IMarketData
    {
        private readonly HttpClient _httpClient;
        private readonly ILog _logger;

        public HttpConnector(HttpClient httpClient, ILog logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task FetchData(string currencyPair, EventHandler<T> handler)
        {
            try
            {
                var httpResponseMessage = await _httpClient.GetAsync(currencyPair);
                var content = await httpResponseMessage.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<T>(content);
                data.CurrencyPair = currencyPair;
                handler(this, data);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
