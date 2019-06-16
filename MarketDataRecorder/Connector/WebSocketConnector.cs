using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MarketDataRecorder.DataModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MarketDataRecorder.Connector
{
    public class WebSocketConnector<T> : IConnector<T> where T : IMarketData
    {
        private readonly ClientWebSocket _webSocketClient;
        private readonly int _reconnectDelayMilliseconds;
        private readonly Uri _server;
        private readonly ILog _logger;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public WebSocketConnector(ClientWebSocket webSocketClient, int reconnectDelayMilliseconds, Uri server, ILog logger)
        {
            _webSocketClient = webSocketClient;
            _reconnectDelayMilliseconds = reconnectDelayMilliseconds;
            _server = server;
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task FetchData(string currencyPair, EventHandler<T> handler)
        {
            var buffer = await Connect(currencyPair);

            while (_webSocketClient.State == WebSocketState.Open)
            {
                var message = await ReadMessage(buffer);
                if (!await CheckConnectivity(currencyPair, handler, message)) break;
                var data = JsonConvert.DeserializeObject<T>(JObject.Parse(message)["data"].ToString());
                data.CurrencyPair = currencyPair;
                handler(this, data);
            }
        }

        private async Task<bool> CheckConnectivity(string currencyPair, EventHandler<T> handler, string message)
        {
            var eventName = JObject.Parse(message)["event"].ToString();
            _logger.Info($"Received event: {eventName}");
            if (eventName == "bts:request_reconnect")
            {
                await Task.Run(async delegate
                {
                    _logger.Warn($"Reconnecting {currencyPair} in {_reconnectDelayMilliseconds} milliseconds");
                    await Task.Delay(_reconnectDelayMilliseconds);
                    return FetchData(currencyPair, handler);
                });
                return false;
            }

            return true;
        }

        private async Task<ArraySegment<byte>> Connect(string currencyPair)
        {
            await _webSocketClient.ConnectAsync(_server, _cancellationTokenSource.Token);
            var request = await SendRequest(currencyPair, "bts:subscribe");
            _logger.Info($"Request sent: {request}");

            var buffer = new ArraySegment<byte>(new byte[512]);
            var message = await ReadMessage(buffer);
            _logger.Info($"Received status: {message}");
            return buffer;
        }

        private async Task<string> ReadMessage(ArraySegment<byte> buffer)
        {
            var result = await _webSocketClient.ReceiveAsync(buffer, _cancellationTokenSource.Token);
            byte[] messageBytes = buffer.Skip(buffer.Offset).Take(result.Count).ToArray();
            return Encoding.UTF8.GetString(messageBytes);
        }

        private async Task<string> SendRequest(string currencyPair, string eventName)
        {
            var request = CreateRequest(currencyPair, eventName);
            await _webSocketClient.SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes(request),
                    0,
                    request.Length),
                WebSocketMessageType.Text,
                true,
                _cancellationTokenSource.Token);
            return request;
        }

        private static string CreateRequest(string currencyPair, string eventName)
        {
            var request = JObject.FromObject(new StreamingDataRequest
            {
                EventName = eventName,
                Data = new RequestData {Channel = $"live_trades_{currencyPair}"}
            }).ToString();
            return request;
        }

        public async void Dispose()
        {
            await _webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, _cancellationTokenSource.Token);
            _webSocketClient.Dispose();
            _cancellationTokenSource.Dispose();
        }
    }
}
