using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyClient.Driver;
using Moq;
using System.Collections.Concurrent;
using System.Threading;
using Xunit;
using MyDriver;

namespace MyClient.Tests
{
    public class MyRequestSenderTest
    {
        internal Mock<IMyDriverClient> _clientMock;
        internal Mock<IMyConnector> _connectorMock;
        internal BlockingCollection<MyRequest> _pendingRequests;
        internal CancellationTokenSource _cts;

        internal MyRequestSender _sender;

        public MyRequestSenderTest()
        {
            this._clientMock = new Mock<IMyDriverClient>(MockBehavior.Strict);

            this._connectorMock = new Mock<IMyConnector>(MockBehavior.Strict);
            this._connectorMock.Setup(c => c.Client).Returns(this._clientMock.Object);

            this._pendingRequests = new BlockingCollection<MyRequest>();
            this._cts = new CancellationTokenSource();

            this._sender = new MyRequestSender(this._connectorMock.Object, this._pendingRequests, this._cts.Token);
        }

        public class Process : MyRequestSenderTest
        {
            [Fact]
            public void SubscribeSucceeded_AddQueryCalled()
            {
                var requests = Enumerable.Range(0, 10).Select(i => new MyRequest(MyRequestType.Subscribe, new MySubscription(null))).ToList();

                foreach (var req in requests)
                {
                    this._pendingRequests.Add(req);
                    this._clientMock.Setup(c => c.AddQuery(req.Subscription.QueryID));
                }

                this._pendingRequests.CompleteAdding();

                this._sender.Process();

                foreach (var req in requests)
                {
                    this._clientMock.Verify(c => c.AddQuery(req.Subscription.QueryID), Times.Once());
                }
            }

            [Fact]
            public void UnsubscribeSucceeded_RemoveQueryCalled()
            {
                var requests = Enumerable.Range(0, 10).Select(i => new MyRequest(MyRequestType.Unsubscribe, new MySubscription(null))).ToList();

                foreach (var req in requests)
                {
                    this._pendingRequests.Add(req);
                    this._clientMock.Setup(c => c.RemoveQuery(req.Subscription.QueryID));
                }

                this._pendingRequests.CompleteAdding();

                this._sender.Process();

                foreach (var req in requests)
                {
                    this._clientMock.Verify(c => c.RemoveQuery(req.Subscription.QueryID), Times.Once());
                }
            }

            [Fact]
            public void SubscribeFailed_CloseClientCalled()
            {
                this._clientMock.Setup(c => c.AddQuery(It.IsAny<int>())).Throws(new MyDriverException("Unit Test"));
                this._connectorMock.Setup(c => c.CloseClient());

                this._pendingRequests.Add(new MyRequest(MyRequestType.Subscribe, new MySubscription(null)));

                this._sender.Process();

                this._connectorMock.Verify(c => c.CloseClient(), Times.Once());
            }
        }
    }
}
