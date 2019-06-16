using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MarketDataRecorder.Connector;
using MarketDataRecorder.DataModel;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MarketDataRecorder.Tests.Connector
{
    public class HttpClientConnectorTest
    {
        private HttpClient _httpClient;
        private Mock<HttpMessageHandler> _handlerMock;
        private HttpConnector<SnapshotData> _objectToTest;
        private string _currencyPair = "eurusd";

        [SetUp]
        public void SetUp()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(new SnapshotData())),
                })
                .Verifiable();

            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri($"http://{nameof(HttpClientConnectorTest)}.com/"),
            };

            _objectToTest = new HttpConnector<SnapshotData>(_httpClient, new Mock<ILog>().Object);
        }

        [TearDown]
        public void TearDown()
        {
            _objectToTest.Dispose();
            _httpClient.Dispose();
        }

        [Test]
        public async Task WhenFetchDataThenShouldGetAsync()
        {
            await _objectToTest.FetchData(_currencyPair, (sender, data) =>
            {
                Assert.IsNotNull(data);
            });
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get
                    && req.RequestUri == new Uri(_httpClient.BaseAddress + _currencyPair)
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
