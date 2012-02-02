using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace MyDriver
{
    public sealed class MyDriverClient : IDisposable
    {
        private readonly Random _random = new Random(DateTime.Now.Millisecond);
        private readonly HashSet<int> _queryIds = new HashSet<int>();
        private readonly BlockingCollection<MyData> _dataCollection = new BlockingCollection<MyData>(10000);
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public MyDriverClient(string uri)
        {
            new Thread(FeedData).Start();
        }

        public void Connect()
        {
            if (this._random.NextDouble() < 0.1)
            {
                throw new MyDriverException("Error occurred when add query.");
            }
        }

        public void AddQuery(int queryId)
        {
            if (this._random.NextDouble() < 0.02)
            {
                throw new MyDriverException("Error occurred when add query.");
            }

            lock (this._queryIds)
            {
                if (this._queryIds.Add(queryId))
                {
                    this._dataCollection.Add(new MyData(queryId, "begin"));
                }
            }
        }

        public void RemoveQuery(int queryId)
        {
            if (this._random.NextDouble() < 0.02)
            {
                throw new MyDriverException("Error occurred when remove query.");
            }

            lock (this._queryIds)
            {
                this._queryIds.Remove(queryId);
            }
        }

        public void Dispose()
        {
            this._cts.Cancel();
        }

        private void FeedData()
        {
            while (!this._cts.IsCancellationRequested)
            {
                Thread.Sleep(500);

                lock (this._queryIds)
                {
                    foreach (var id in this._queryIds)
                    {
                        this._dataCollection.Add(new MyData(id, this._random.Next().ToString()));
                    }
                }
            }
        }

        public MyData Receive()
        {
            if (this._cts.IsCancellationRequested) return null;

            if (this._random.NextDouble() < 0.01)
            {
                throw new MyDriverException("Error occurred when receive.");
            }

            try
            {
                return this._dataCollection.Take(this._cts.Token);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }
    }
}
