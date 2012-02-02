using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using MyClient.Threading;
using log4net;

namespace MyClient
{
    internal interface IMySubscriptionManagerFactory
    {
        IMySubscriptionManager Create(string name, IMyConnector connector);
    }

    internal interface IMySubscriptionManager : IDisposable
    {
        void StartConnecting();

        void OnConnected(object sender, EventArgs args);

        void OnDisconnected(object sender, EventArgs args);

        void AddSubscription(MySubscription subscription);

        bool RemoveSubscription(int subscriptionId);
    }

    internal class MySubscriptionManager : IMySubscriptionManager
    {
        private class Factory : IMySubscriptionManagerFactory
        {
            public IMySubscriptionManager Create(string name, IMyConnector connector)
            {
                return new MySubscriptionManager(name, connector);
            }
        }

        public static readonly IMySubscriptionManagerFactory DefaultFactory = new Factory();

        private static readonly ILog Logger = LogManager.GetLogger(typeof(MySubscriptionManager));

        private string _name;
        private IMyConnector _connector;

        private IDictionary<int, MySubscription> _subscriptions;
        private IThreadUtils _threadUtils;
        private IMyRequestSenderFactory _requestSenderFactory;
        private IMyDataReceiverFactory _receiverFactory;
        private IMyDataDispatcherFactory _dispatcherFactory;

        private BlockingCollection<MyRequest> _pendingRequests;
        private CancellationTokenSource _cts;

        internal MySubscriptionManager(
            string name,
            IMyConnector connector,
            /* Dependencies */
            IDictionary<int, MySubscription> subscriptions,
            IThreadUtils threadUtils,
            IMyRequestSenderFactory requestSenderFactory,
            IMyDataReceiverFactory receiverFactory,
            IMyDataDispatcherFactory dispatcherFactory)
        {
            this._name = name;
            this._connector = connector;

            this._subscriptions = subscriptions;
            this._threadUtils = threadUtils;
            this._requestSenderFactory = requestSenderFactory;
            this._receiverFactory = receiverFactory;
            this._dispatcherFactory = dispatcherFactory;
        }

        public MySubscriptionManager(string name, IMyConnector connector)
            : this(
                name,
                connector,
                new ConcurrentDictionary<int, MySubscription>(),
                ThreadUtils.Instance,
                MyRequestSender.DefaultFactory,
                MyDataReceiver.DefaultFactory,
                MyDataDispatcher.DefaultFactory) { }

        public void StartConnecting()
        {
            this._threadUtils.StartNew("MyConnector_" + this._name, this._connector.Connect);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddSubscription(MySubscription subscription)
        {
            this._subscriptions.Add(subscription.QueryID, subscription);

            if (this._pendingRequests != null)
            {
                this._pendingRequests.Add(new MyRequest(MyRequestType.Subscribe, subscription));
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool RemoveSubscription(int subscriptionId)
        {
            MySubscription subscription;

            if (this._subscriptions.TryGetValue(subscriptionId, out subscription))
            {
                this._subscriptions.Remove(subscriptionId);
            }
            else
            {
                return false;
            }

            if (this._pendingRequests != null)
            {
                this._pendingRequests.Add(new MyRequest(MyRequestType.Unsubscribe, subscription));
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void OnConnected(object sender, EventArgs args)
        {
            Logger.Info("Connected, start workers.");

            this._pendingRequests = new BlockingCollection<MyRequest>();
            foreach (var s in this._subscriptions.Values)
            {
                this._pendingRequests.Add(new MyRequest(MyRequestType.Subscribe, s));
            }

            this._cts = new CancellationTokenSource();

            var requestSender = this._requestSenderFactory.Create(this._connector, this._pendingRequests, this._cts.Token);
            this._threadUtils.StartNew("MyRequestSender_" + this._name, requestSender.Process);

            var receiver = this._receiverFactory.Create(this._connector);
            this._threadUtils.StartNew("MyDataReceiver_" + this._name, receiver.Process);

            var dispatcher = this._dispatcherFactory.Create(this._subscriptions, receiver.DataProduced, receiver.CancellationToken);
            this._threadUtils.StartNew("MyDataDispatcher_" + this._name, dispatcher.Process);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void OnDisconnected(object sender, EventArgs args)
        {
            Logger.Info("Disconnected, stop exiting workers and reconnect.");

            this._pendingRequests = null;

            this._cts.Cancel();
            this._cts = null;

            this._threadUtils.StartNew("MyConnector_" + this._name, this._connector.Connect);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Dispose()
        {
            this._connector.CloseClient();

            if (this._pendingRequests != null)
            {
                this._pendingRequests = null;

                this._cts.Cancel();
                this._cts = null;
            }
        }
    }
}
