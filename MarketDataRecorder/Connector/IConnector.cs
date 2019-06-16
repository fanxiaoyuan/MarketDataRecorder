using System;
using System.Threading.Tasks;

namespace MarketDataRecorder.Connector
{
    public interface IConnector<T> : IDisposable
    {
        Task FetchData(string currencyPair, EventHandler<T> handler);
    }
}
