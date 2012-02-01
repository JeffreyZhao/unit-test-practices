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
        private IMySubscriptionManager _subscriptionManager;

        internal MyConnection(
            string name,
            string[] uris,
            IMyConnectorFactory connectorFactory,
            IMySubscriptionManagerFactory subscriptionManagerFactory)
        {
            this._name = name;
            this._uris = uris;

            var connector = connectorFactory.Create(this._uris, this);
            this._subscriptionManager = subscriptionManagerFactory.Create(this._name, connector);

            this.Connected += this._subscriptionManager.OnConnected;
            this.Disconnected += this._subscriptionManager.OnDisconnected;
        }

        public MyConnection(string name, string[] uris)
            : this(name, uris, MyConnector.DefaultFactory, MySubscriptionManager.DefaultFactory)
        { }

        public MyConnection(string[] uris)
            : this(Interlocked.Increment(ref _nameSeed).ToString(), uris)
        { }

        public void Open()
        {
            this._subscriptionManager.StartConnecting();
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
