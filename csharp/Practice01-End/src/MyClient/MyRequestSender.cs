using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using log4net;
using MyDriver;

namespace MyClient
{
    internal interface IMyRequestSender
    {
        void Process();
    }

    internal class MyRequestSender : IMyRequestSender
    {
        private static ILog Logger = LogManager.GetLogger(typeof(MyRequestSender));

        private IMyConnector _connector;
        private BlockingCollection<MyRequest> _pendingRequests;
        private CancellationToken _ct;

        public MyRequestSender(IMyConnector connector, BlockingCollection<MyRequest> pendingRequests, CancellationToken ct)
        {
            this._connector = connector;
            this._pendingRequests = pendingRequests;
            this._ct = ct;
        }

        public void Process()
        {
            var client = this._connector.Client;
            if (client == null)
            {
                Logger.Info("Connector isn't prepared, stop processing.");
                return;
            }

            while (true)
            {
                if (this._pendingRequests.IsCompleted)
                {
                    Logger.Info("Requests collection is completed, stop processing");
                    return;
                }

                MyRequest request;
                try
                {
                    request = this._pendingRequests.Take(this._ct);
                }
                catch (OperationCanceledException)
                {
                    Logger.Info("Receiving pending requests canceled, stop processing.");
                    return;
                }

                Logger.Info("Request received: " + request.Subscription.QueryID);

                if (request.Type == MyRequestType.Subscribe)
                {
                    try
                    {
                        client.AddQuery(request.Subscription.QueryID);
                    }
                    catch (MyDriverException ex)
                    {
                        Logger.Error("Exception occurred when adding query, close the client and stop processing.", ex);

                        this._connector.CloseClient();
                        return;
                    }
                }
                else
                {
                    try
                    {
                        client.RemoveQuery(request.Subscription.QueryID);
                    }
                    catch (MyDriverException ex)
                    {
                        Logger.Error("Exception occurred when removing query, close the client and stop processing.", ex);

                        this._connector.CloseClient();
                        return;
                    }
                }
            }
        }
    }
}
