using System;
using System.Collections.Generic;
using System.Linq;
using MarketDataRecorder.Connector;
using MarketDataRecorder.DataModel;

namespace MarketDataRecorder
{
    public class StreamingDataProcessor : IDisposable
    {
        private readonly string[] _currencyPairs;
        private readonly List<IConnector<StreamingData>> _connectors;
        private readonly IWebSocketConnectorFactory _connectorFactory;
        private readonly DataExporter _exporter;

        public StreamingDataProcessor(IEnumerable<string> currencyPairs, IWebSocketConnectorFactory connectorFactory, DataExporter exporter)
        {
            _currencyPairs = currencyPairs.ToArray();
            _connectorFactory = connectorFactory;
            _exporter = exporter;
            _connectors = new List<IConnector<StreamingData>>();
        }

        public void Start()
        {
            foreach (var currencyPair in _currencyPairs)
            {
                var connector = _connectorFactory.Create<StreamingData>(currencyPair);
                _connectors.Add(connector);

                connector.FetchData(currencyPair, Handler);
            }
        }

        private void Handler(object sender, StreamingData e)
        {
            _exporter.Export(new[]{e});
        }

        public void Stop()
        {
            foreach (var connector in _connectors)
            {
                connector.Dispose();
            }
            _connectors.Clear();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
