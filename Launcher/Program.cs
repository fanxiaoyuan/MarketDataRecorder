using System;
using System.Configuration;
using System.Net.Http;
using MarketDataRecorder;
using MarketDataRecorder.Connector;
using MarketDataRecorder.DataModel;

namespace Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var currencyPairs = ConfigurationManager.AppSettings["currency_pairs"].Split(",");
            var pathRoot = ConfigurationManager.AppSettings["root_directory"];
            var snapshotProcessor = CreateSnapshotProcessor(currencyPairs, pathRoot);
            snapshotProcessor.Start();

            var streamingProcessor = CreateStreamingProcessor(currencyPairs, pathRoot);
            streamingProcessor.Start();

            Console.ReadLine();
            streamingProcessor.Stop();
            snapshotProcessor.Stop();
        }

        private static SnapshotMarketDataProcessor CreateSnapshotProcessor(string[] currencyPairs, string pathRoot)
        {
            return new SnapshotMarketDataProcessor(currencyPairs, Int32.Parse(ConfigurationManager.AppSettings["interval"]),
        new HttpConnector<SnapshotData>(new HttpClient
                    {BaseAddress = new Uri(ConfigurationManager.AppSettings["snapshot_uri"]) }, LoggerProvider.GetLogger<SnapshotMarketDataProcessor>()),
                new DataExporter($"{pathRoot}\\{ConfigurationManager.AppSettings["snapshot_folder"]}"));
        }

        private static StreamingDataProcessor CreateStreamingProcessor(string[] currencyPairs, string pathRoot)
        {
            return new StreamingDataProcessor(currencyPairs,
                new WebSocketConnectorFactory(new Uri(ConfigurationManager.AppSettings["streaming_uri"]), LoggerProvider.GetLogger<StreamingDataProcessor>()),
                new DataExporter($"{pathRoot}\\{ConfigurationManager.AppSettings["streaming_folder"]}"));
        }
    }
}
