using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyClient.Driver;
using log4net;
using MyClient.Threading;
using System.Runtime.CompilerServices;

namespace MyClient
{
    internal interface IMyConnectorFactory
    {
        IMyConnector Create(string[] uris, IConnectionEventFirer eventFirer);
    }

    internal interface IMyConnector
    {
        void Connect();

        void CloseClient();

        IMyDriverClient Client { get; }
    }

    internal class MyConnector : IMyConnector
    {
        private class Factory : IMyConnectorFactory
        {
            public IMyConnector Create(string[] uris, IConnectionEventFirer eventFirer)
            {
                return new MyConnector(uris, eventFirer);
            }
        }

        public static readonly int ReconnectInterval = 3000;

        public static readonly IMyConnectorFactory DefaultFactory = new Factory();

        private static ILog Logger = LogManager.GetLogger(typeof(MyConnection));

        private int _currentUriIndex = 0;
        private bool _closingClient;

        private string[] _uris;
        private IThreadUtils _threadUtils;
        private IConnectionEventFirer _eventFirer;
        private IMyDriverClientFactory _clientFactory;

        internal MyConnector(
            string[] uris,
            IConnectionEventFirer eventFirer,
            IThreadUtils threadUtils,
            IMyDriverClientFactory clientFactory)
        {
            this._uris = uris;
            this._eventFirer = eventFirer;
            this._threadUtils = threadUtils;
            this._clientFactory = clientFactory;
        }

        public MyConnector(string[] uris, IConnectionEventFirer eventFirer)
            : this(uris, eventFirer, ThreadUtils.Instance, MyDriverClientWrapper.DefaultFactory)
        { }

        public IMyDriverClient Client { get; private set; }

        private IMyDriverClient ConnectUntilSucceeded()
        {
            while (true)
            {
                if (this._closingClient) return null;

                var uri = this._uris[this._currentUriIndex];

                Logger.Info("Connecting to " + uri);

                try
                {
                    var client = this._clientFactory.Create(uri);
                    client.Connect();

                    return client;
                }
                catch (Exception ex)
                {
                    Logger.Error("Error when connecting to " + uri, ex);

                    this._threadUtils.Sleep(ReconnectInterval);

                    this._eventFirer.FireConnectFailed();

                    this._currentUriIndex = (this._currentUriIndex + 1) % this._uris.Length;
                }
            }
        }

        public void Connect()
        {
            this._closingClient = false;

            this.Client = this.ConnectUntilSucceeded();

            if (this._closingClient)
            {
                if (this.Client != null)
                {
                    this.CloseClientInternal();
                }
            }
            else
            {
                this._eventFirer.FireConnected();
            }
        }

        private void CloseClientInternal()
        {
            this.Client.Dispose();
            this.Client = null;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void CloseClient()
        {
            this._closingClient = true;

            if (this.Client != null)
            {
                this.CloseClientInternal();

                this._eventFirer.FireDisconnected();
            }
        }
    }
}
