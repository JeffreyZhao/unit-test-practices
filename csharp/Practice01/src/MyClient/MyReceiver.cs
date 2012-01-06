using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace MyClient
{
    public sealed class MyReceiver
    {
        private readonly Random _random = new Random(DateTime.Now.Millisecond);

        private readonly HashSet<int> _queryIds = new HashSet<int>();
        private readonly BlockingCollection<MyData> _dataCollection = new BlockingCollection<MyData>(10000);
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        internal MyReceiver()
        {
            new Thread(FeedData).Start();
        }

        internal void AddQuery(int id)
        {
            lock (this._queryIds)
            {
                this._queryIds.Add(id);
                this._dataCollection.Add(new MyData(id, "begin"));
            }
        }

        internal void RemoveQuery(int id)
        {
            lock (this._queryIds)
            {
                this._queryIds.Remove(id);
            }
        }

        internal void Close()
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
                throw new MyClientException("Error occurred when receive.");
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
