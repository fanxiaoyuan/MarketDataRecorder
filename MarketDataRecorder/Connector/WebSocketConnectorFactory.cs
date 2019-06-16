using System;
using System.Net.WebSockets;
using log4net;
using MarketDataRecorder.DataModel;

namespace MarketDataRecorder.Connector
{
    public interface IWebSocketConnectorFactory
    {
        IConnector<T> Create<T>(string currencyPair) where T : IMarketData;
    }

    public class WebSocketConnectorFactory : IWebSocketConnectorFactory
    {
        private readonly Uri _uri;
        private readonly ILog _logger;

        public WebSocketConnectorFactory(Uri uri, ILog logger)
        {
            _uri = uri;
            _logger = logger;
        }

        public IConnector<T> Create<T>(string currencyPair) where T : IMarketData
        {
            return new WebSocketConnector<T>(new ClientWebSocket(), 5000, _uri, _logger);
        }
    }
}
