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
    internal interface IMyDataReceiverFactory
    {
        IMyDataReceiver Create(IMyConnector connector);
    }

    internal interface IMyDataReceiver
    {
        void Process();

        BlockingCollection<MyData> DataProduced { get; }

        CancellationToken CancellationToken { get; }
    }

    internal class MyDataReceiver : IMyDataReceiver
    {
        private class Factory : IMyDataReceiverFactory
        {
            public IMyDataReceiver Create(IMyConnector connector)
            {
                return new MyDataReceiver(connector);
            }
        }

        public static readonly IMyDataReceiverFactory DefaultFactory = new Factory();

        private static ILog Logger = LogManager.GetLogger(typeof(MyDataReceiver));

        private IMyConnector _connector;
        private CancellationTokenSource _cts;

        public MyDataReceiver(IMyConnector connector)
        {
            this._connector = connector;
            this._cts = new CancellationTokenSource();
            this.DataProduced = new BlockingCollection<MyData>();
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
                MyData data;

                try
                {
                    data = client.Receive();
                }
                catch (MyDriverException ex)
                {
                    Logger.Error("Exception thrown when receiving, close the client and stop processing.", ex);

                    this._connector.CloseClient();
                    this._cts.Cancel();
                    return;
                }

                if (data == null)
                {
                    Logger.Info("The client is closed by others, stop processing.");
                    this._cts.Cancel();
                    return;
                }

                this.DataProduced.Add(data);
            }
        }

        public BlockingCollection<MyData> DataProduced { get; private set; }

        public CancellationToken CancellationToken { get { return this._cts.Token; } }
    }
}
