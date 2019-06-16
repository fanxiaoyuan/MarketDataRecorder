using System;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MarketDataRecorder.Connector;
using MarketDataRecorder.DataModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MarketDataRecorder.Tests
{
    [TestFixture]
    public class BitstampTests
    {
        [Test]
        public async Task TestSnapshotDataReceive()
        {
            using (var client = new HttpClient{BaseAddress = new Uri("https://www.bitstamp.net/api/v2/ticker/") })
            {
                var httpResponseMessage = client.GetAsync("btcusd/").Result;
                Assert.That(httpResponseMessage.IsSuccessStatusCode);

                var content = await httpResponseMessage.Content.ReadAsStringAsync();
                Assert.NotNull(JsonConvert.DeserializeObject<SnapshotData>(content));
            }
        }

        [Test]
        public async Task TestStreamingDataReceive()
        {
            using (var webSocket = new ClientWebSocket())
            {
                var cancellationToken = CancellationToken.None;
                await webSocket.ConnectAsync(new Uri("wss://ws.bitstamp.net"), cancellationToken);
                Assert.That(webSocket.State, Is.EqualTo(WebSocketState.Open));
                var request = JObject.FromObject(new StreamingDataRequest
                {
                    EventName = "bts:subscribe",
                    Data = new RequestData {Channel = "live_trades_btcusd"}
                }).ToString();
                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes(request),
                        0,
                        request.Length),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);

                var buffer = new ArraySegment<byte>(new byte[512]);
                var message = await FetchMessage(webSocket, buffer, cancellationToken);
                Assert.That(JObject.Parse(message)["event"].Value<string>, Is.EqualTo("bts:subscription_succeeded"));

                message = await FetchMessage(webSocket, buffer, cancellationToken);
                Assert.IsNotNull(JsonConvert.DeserializeObject<StreamingData>(message));

                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, cancellationToken);
                Assert.That(webSocket.State, Is.EqualTo(WebSocketState.Closed));
            }
        }

        private static async Task<string> FetchMessage(ClientWebSocket webSocket, ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, cancellationToken);
            byte[] messageBytes = buffer.Skip(buffer.Offset).Take(result.Count).ToArray();
            string message = Encoding.UTF8.GetString(messageBytes);
            return message;
        }
    }
}