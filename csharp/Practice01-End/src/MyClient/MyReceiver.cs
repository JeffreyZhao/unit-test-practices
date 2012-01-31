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
    internal interface IMyReceiver
    {
        void Process();

        BlockingCollection<MyData> DataProduced { get; }

        CancellationToken CancellationToken { get; }
    }

    internal class MyReceiver : IMyReceiver
    {
        private static ILog Logger = LogManager.GetLogger(typeof(MyReceiver));

        private IMyConnector _connector;
        private CancellationTokenSource _cts;

        public MyReceiver(IMyConnector connector)
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
                    Logger.Error("Exception occurred when receiving, close the client and stop processing.", ex);

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
