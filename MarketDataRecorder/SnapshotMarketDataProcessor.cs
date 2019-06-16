using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using MarketDataRecorder.Connector;
using MarketDataRecorder.DataModel;
using Timer = System.Timers.Timer;

namespace MarketDataRecorder
{
    public class SnapshotMarketDataProcessor : IDisposable
    {
        private readonly string[] _currencyPairs;
        private readonly IConnector<SnapshotData> _connector;
        private readonly DataExporter _exporter;
        private readonly Timer _timer;
        private static int _requestId;
        public SnapshotMarketDataProcessor(IEnumerable<string> currencyPairs, int intervalMilliseconds, IConnector<SnapshotData> connector, DataExporter exporter)
        {
            _currencyPairs = currencyPairs.ToArray();
            _connector = connector;
            _exporter = exporter;
            _timer = new Timer(intervalMilliseconds) {AutoReset = true};
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var snapshots = new List<SnapshotData>();
            var tasks = _currencyPairs.Select(currencyPair =>
            {
                Interlocked.Increment(ref _requestId);
                return _connector.FetchData(currencyPair, (o, data) => snapshots.Add(data));
            }).ToArray();
            Task.WaitAll(tasks);
            _exporter.Export(snapshots);
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Dispose()
        {
            _timer.Elapsed -= _timer_Elapsed;
            _timer.Dispose();
            _connector.Dispose();
        }
    }
}
