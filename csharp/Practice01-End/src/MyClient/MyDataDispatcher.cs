using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using MyDriver;
using System.Threading;
using log4net;

namespace MyClient
{
    internal class MyDataDispatcher
    {
        public static readonly ILog Logger = LogManager.GetLogger(typeof(MyDataDispatcher));

        private IDictionary<int, MySubscription> _subscriptions;
        private BlockingCollection<MyData> _dataCollection;
        private CancellationToken _ct;

        public MyDataDispatcher(
            IDictionary<int, MySubscription> subscriptions,
            BlockingCollection<MyData> dataCollection,
            CancellationToken ct)
        {
            this._subscriptions = subscriptions;
            this._dataCollection = dataCollection;
            this._ct = ct;
        }

        public void Process()
        {
            while (true)
            {
                if (this._dataCollection.IsCompleted)
                {
                    Logger.Info("Data collection is completed, stop processing");
                    return;
                }

                MyData data;
                try
                {
                    data = this._dataCollection.Take(this._ct);
                }
                catch (OperationCanceledException)
                {
                    Logger.Info("Taking data canceled, stop processing.");
                    return;
                }

                MySubscription subscription;
                if (!this._subscriptions.TryGetValue(data.QueryID, out subscription))
                {
                    Logger.Warn("Unexpected subscription id: " + data.QueryID);
                    continue;
                }

                try
                {
                    if (data.Value == "begin")
                    {
                        subscription.Subscriber.OnBegin();
                    }
                    else
                    {
                        subscription.Subscriber.OnMessage(data.Value);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception thrown when dispatch data: " + data, ex);
                }
            }
        }
    }
}
