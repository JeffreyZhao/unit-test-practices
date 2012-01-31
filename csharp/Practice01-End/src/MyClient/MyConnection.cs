using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MyClient.Threading;

namespace MyClient
{
    public class MyConnection : IConnectionEventFirer, IDisposable
    {
        private static int _nameSeed = 0;

        public readonly static int ReconnectInterval = 3000;

        private string _name;
        private string[] _uris;
        private IThreadUtils _threadUtils;
        private IMySubscriptionManager _subscriptionManager;
        private IMyConnector _connector;

        internal MyConnection(
            string name,
            string[] uris,
            IThreadUtils threadUtils,
            IMySubscriptionManagerFactory subscriptionManagerFactory,
            IMyConnectorFactory connectorFactory)
        {
            this._name = name;
            this._uris = uris;

            this._threadUtils = threadUtils;
            this._subscriptionManager = subscriptionManagerFactory.Create(this._name);
            this._connector = connectorFactory.Create(this._uris, this);

            this.Connected += this._subscriptionManager.OnConnected;
            this.Disconnected += this._subscriptionManager.OnDisconnected;
        }

        public MyConnection(string name, string[] uris)
            : this(name, uris, ThreadUtils.Instance, MySubscriptionManager.DefaultFactory, MyConnector.DefaultFactory)
        { }

        public MyConnection(string[] uris)
            : this(Interlocked.Increment(ref _nameSeed).ToString(), uris)
        { }

        public void Open()
        {
            this._threadUtils.StartNew("MyConnector_" + this._name, this._connector.Connect);
        }

        public int Subscribe(IMySubscriber subscriber)
        {
            var subscription = new MySubscription(subscriber);
            this._subscriptionManager.AddSubscription(subscription);
            return subscription.QueryID;
        }

        public void Unsubscribe(int subscriptionId)
        {
            this._subscriptionManager.RemoveSubscription(subscriptionId);
        }

        public event EventHandler Connected;

        public event EventHandler ConnectFailed;

        public event EventHandler Disconnected;

        public void Dispose()
        {
            this.Connected -= this._subscriptionManager.OnConnected;
            this.Disconnected -= this._subscriptionManager.OnDisconnected;

            this._subscriptionManager.Dispose();
            this._connector.CloseClient();
        }

        void IConnectionEventFirer.FireConnected()
        {
            var connected = this.Connected;
            if (connected == null) return;

            connected(this, EventArgs.Empty);
        }

        void IConnectionEventFirer.FireConnectFailed()
        {
            var connectFailed = this.ConnectFailed;
            if (connectFailed == null) return;

            connectFailed(this, EventArgs.Empty);
        }

        void IConnectionEventFirer.FireDisconnected()
        {
            var disconnected = this.Disconnected;
            if (disconnected == null) return;

            disconnected(this, EventArgs.Empty);
        }
    }
}
