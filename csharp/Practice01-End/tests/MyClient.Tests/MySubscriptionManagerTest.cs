using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyClient.Tests.Threading;
using Moq;
using Xunit;
using System.Threading;
using System.Collections.Concurrent;
using MyDriver;

namespace MyClient.Tests
{
    public class MySubscriptionManagerTest
    {
        internal string _name;

        internal Mock<IMyConnector> _connectorMock;
        internal IDictionary<int, MySubscription> _subscriptions;
        internal Mock<DelayThreadUtils> _threadUtilsMock;

        internal Mock<IMyRequestSender> _requestSenderMock;
        internal Mock<IMyRequestSenderFactory> _requestSenderFactoryMock;

        internal Mock<IMyDataReceiver> _receiverMock;
        internal Mock<IMyDataReceiverFactory> _receiverFactoryMock;

        internal Mock<IMyDataDispatcher> _dispatcherMock;
        internal Mock<IMyDataDispatcherFactory> _dispatcherFactoryMock;

        internal MySubscriptionManager _manager;

        public MySubscriptionManagerTest()
        {
            this._name = "MyTest";

            this._connectorMock = new Mock<IMyConnector>(MockBehavior.Strict);
            this._subscriptions = new Dictionary<int, MySubscription>();
            this._threadUtilsMock = new Mock<DelayThreadUtils>() { CallBase = true };

            this._requestSenderMock = new Mock<IMyRequestSender>(MockBehavior.Strict);
            this._requestSenderFactoryMock = new Mock<IMyRequestSenderFactory>(MockBehavior.Strict);

            this._receiverMock = new Mock<IMyDataReceiver>(MockBehavior.Strict);
            this._receiverFactoryMock = new Mock<IMyDataReceiverFactory>(MockBehavior.Strict);

            this._dispatcherMock = new Mock<IMyDataDispatcher>(MockBehavior.Strict);
            this._dispatcherFactoryMock = new Mock<IMyDataDispatcherFactory>(MockBehavior.Strict);

            this._manager = new MySubscriptionManager(
                this._name,
                this._connectorMock.Object,
                this._subscriptions,
                this._threadUtilsMock.Object,
                this._requestSenderFactoryMock.Object,
                this._receiverFactoryMock.Object,
                this._dispatcherFactoryMock.Object);
        }

        private void SetupRequestSenderMocks(Action<IMyConnector, BlockingCollection<MyRequest>, CancellationToken> factoryCallback)
        {
            this._requestSenderMock.Setup(s => s.Process());
            this._requestSenderFactoryMock
                .Setup(f => f.Create(this._connectorMock.Object, It.IsAny<BlockingCollection<MyRequest>>(), It.IsAny<CancellationToken>()))
                .Returns(this._requestSenderMock.Object)
                .Callback(factoryCallback ?? ((_0, _1, _2) => { }));
        }

        private void SetupReceiverMocks()
        {
            this._receiverMock.Setup(r => r.Process());
            this._receiverMock.Setup(r => r.DataProduced).Returns(new BlockingCollection<MyData>());
            this._receiverMock.Setup(r => r.CancellationToken).Returns(new CancellationToken());
            this._receiverFactoryMock.Setup(f => f.Create(this._connectorMock.Object)).Returns(this._receiverMock.Object);
        }

        private void SetupDispatcherMocks()
        {
            var receiver = this._receiverMock.Object;

            this._dispatcherMock.Setup(d => d.Process());
            this._dispatcherFactoryMock
                .Setup(f => f.Create(this._subscriptions, receiver.DataProduced, receiver.CancellationToken))
                .Returns(this._dispatcherMock.Object);
        }

        public class StartConnecting : MySubscriptionManagerTest
        {
            [Fact]
            public void Call_DelegateToConnectorInNewThread()
            {
                this._connectorMock.Setup(c => c.Connect());

                this._manager.StartConnecting();

                this._threadUtilsMock.Verify(tu => tu.StartNew("MyConnector_" + this._name, It.IsAny<ThreadStart>()), Times.Once());
                this._connectorMock.Verify(c => c.Connect(), Times.Never());

                this._threadUtilsMock.Object.Execute();
                this._connectorMock.Verify(c => c.Connect(), Times.Once());
            }
        }

        public class OnConnected : MySubscriptionManagerTest
        {
            [Fact]
            public void Call_CreateObjectsAndStartInNewThreads_PendingCollectionHasElements()
            {
                var subscriptions = Enumerable.Range(0, 10).Select(_ => new MySubscription(null)).OrderBy(s => s.QueryID).ToList();
                subscriptions.ForEach(s => this._subscriptions.Add(s.QueryID, s));

                this.SetupRequestSenderMocks((_0, pendingRequests, ct) =>
                {
                    var requests = pendingRequests.Take(pendingRequests.Count).OrderBy(s => s.Subscription.QueryID).ToList();
                    Assert.Equal(subscriptions.Count, requests.Count);
                    for (var i = 0; i < requests.Count; i++)
                    {
                        Assert.Equal(MyRequestType.Subscribe, requests[i].Type);
                        Assert.Same(subscriptions[i], requests[i].Subscription);
                    }

                    Assert.False(ct.IsCancellationRequested);
                });

                this.SetupReceiverMocks();
                this.SetupDispatcherMocks();

                this._manager.OnConnected(null, EventArgs.Empty);

                this._threadUtilsMock.Verify(tu => tu.StartNew("MyRequestSender_" + this._name, It.IsAny<ThreadStart>()), Times.Once());
                this._threadUtilsMock.Verify(tu => tu.StartNew("MyDataReceiver_" + this._name, It.IsAny<ThreadStart>()), Times.Once());
                this._threadUtilsMock.Verify(tu => tu.StartNew("MyDataDispatcher_" + this._name, It.IsAny<ThreadStart>()), Times.Once());

                this._requestSenderMock.Verify(s => s.Process(), Times.Never());
                this._receiverMock.Verify(r => r.Process(), Times.Never());
                this._dispatcherMock.Verify(d => d.Process(), Times.Never());

                this._threadUtilsMock.Object.Execute();

                this._requestSenderMock.Verify(s => s.Process(), Times.Once());
                this._receiverMock.Verify(r => r.Process(), Times.Once());
                this._dispatcherMock.Verify(d => d.Process(), Times.Once());
            }
        }

        public class OnDisconnected : MySubscriptionManagerTest
        {
            [Fact]
            public void Call_TokenCanceled_ReconnectInNewThread()
            {
                CancellationToken ctPassed = default(CancellationToken);

                this.SetupRequestSenderMocks((_0, _1, ct) => { ctPassed = ct; });
                this.SetupReceiverMocks();
                this.SetupDispatcherMocks();

                this._manager.OnConnected(null, EventArgs.Empty);
                this._manager.OnDisconnected(null, EventArgs.Empty);

                Assert.True(ctPassed.IsCancellationRequested);

                this._threadUtilsMock.Verify(tu => tu.StartNew("MyConnector_" + this._name, It.IsAny<ThreadStart>()), Times.Once());

                this._connectorMock.Setup(c => c.Connect());
                this._threadUtilsMock.Object.Execute();

                this._connectorMock.Verify(c => c.Connect(), Times.Once());
            }
        }

        public class AddSubscription : MySubscriptionManagerTest
        {
            [Fact]
            public void Call_AddToSubscriptions()
            {
                var subscription = new MySubscription(null);
                this._manager.AddSubscription(subscription);

                Assert.Same(subscription, this._subscriptions[subscription.QueryID]);
            }

            [Fact]
            public void Connected_SubscribeRequestAdded()
            {
                BlockingCollection<MyRequest> requestCollection = null;

                this.SetupRequestSenderMocks((_0, pendingRequests, _1) =>
                {
                    requestCollection = pendingRequests;
                });

                this.SetupReceiverMocks();
                this.SetupDispatcherMocks();

                var subscription = new MySubscription(null);

                this._manager.OnConnected(null, EventArgs.Empty);
                this._manager.AddSubscription(subscription);

                var request = requestCollection.First();
                Assert.Equal(MyRequestType.Subscribe, request.Type);
                Assert.Same(subscription, request.Subscription);
            }
        }

        public class RemoveSubscription : MySubscriptionManagerTest
        {
            [Fact]
            public void Call_RemoveFromSubscriptions()
            {
                var subscription = new MySubscription(null);
                this._subscriptions.Add(subscription.QueryID, subscription);

                Assert.True(this._manager.RemoveSubscription(subscription.QueryID));
                Assert.False(this._subscriptions.ContainsKey(subscription.QueryID));
            }

            [Fact]
            public void Connected_UnsubscribeRequestAdded()
            {
                BlockingCollection<MyRequest> requestCollection = null;

                this.SetupRequestSenderMocks((_0, pendingRequests, _1) =>
                {
                    requestCollection = pendingRequests;
                });

                this.SetupReceiverMocks();
                this.SetupDispatcherMocks();

                this._manager.OnConnected(null, EventArgs.Empty);

                var subscription = new MySubscription(null);
                this._subscriptions.Add(subscription.QueryID, subscription);

                this._manager.RemoveSubscription(subscription.QueryID);

                var request = requestCollection.First();
                Assert.Equal(MyRequestType.Unsubscribe, request.Type);
                Assert.Same(subscription, request.Subscription);
            }
        }

        public class Dispose : MySubscriptionManagerTest
        {
            [Fact]
            public void Call_CloseConnector()
            {
                this._connectorMock.Setup(c => c.CloseClient());

                this._manager.Dispose();

                this._connectorMock.Verify(c => c.CloseClient(), Times.Once());
            }

            [Fact]
            public void Connected_CloseConnector_TokenCanceled()
            {
                this._connectorMock.Setup(c => c.CloseClient());

                CancellationToken ctPassed = default(CancellationToken);

                this.SetupRequestSenderMocks((_0, _1, ct) => { ctPassed = ct; });
                this.SetupReceiverMocks();
                this.SetupDispatcherMocks();

                this._manager.OnConnected(null, EventArgs.Empty);
                this._manager.Dispose();

                Assert.True(ctPassed.IsCancellationRequested);
                this._connectorMock.Verify(c => c.CloseClient(), Times.Once());
            }
        }
    }
}
