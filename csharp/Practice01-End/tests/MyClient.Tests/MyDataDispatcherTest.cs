using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyDriver;
using System.Collections.Concurrent;
using System.Threading;
using Moq;
using Xunit;

namespace MyClient.Tests
{
    public class MyDataDispatcherTest
    {
        internal Mock<IMySubscriber> _subscriberMock;

        internal IDictionary<int, MySubscription> _subscriptions;
        internal BlockingCollection<MyData> _dataCollection;
        internal CancellationTokenSource _cts;
        internal MyDataDispatcher _dispatcher;

        public MyDataDispatcherTest()
        {
            this._subscriberMock = new Mock<IMySubscriber>(MockBehavior.Strict);

            var subscription = new MySubscription(this._subscriberMock.Object);
            this._subscriptions = new Dictionary<int, MySubscription>
            {
                { subscription.QueryID, subscription }
            };

            this._dataCollection = new BlockingCollection<MyData>();
            this._cts = new CancellationTokenSource();
            this._dispatcher = new MyDataDispatcher(this._subscriptions, this._dataCollection, this._cts.Token);
        }

        public class Process : MyDataDispatcherTest
        {
            [Fact]
            public void DataReceived_DispatchedCorrectly()
            {
                var queryId = this._subscriptions.Keys.Single();

                this._dataCollection.Add(new MyData(queryId, "begin"));
                this._subscriberMock.Setup(s => s.OnBegin());

                for (var i = 0; i < 10; i++)
                {
                    this._dataCollection.Add(new MyData(queryId, i.ToString()));
                    this._subscriberMock.Setup(s => s.OnMessage(i.ToString()));
                }
                this._dataCollection.CompleteAdding();

                this._dispatcher.Process();

                this._subscriberMock.Verify(s => s.OnBegin(), Times.Once());
                for (var i = 0; i < 10; i++)
                {
                    this._subscriberMock.Verify(s => s.OnMessage(i.ToString()), Times.Once());
                }
            }

            [Fact]
            public void UnexpectedQueryID_Ignored()
            {
                var queryId = this._subscriptions.Keys.Single() + 1;

                this._dataCollection.Add(new MyData(queryId, "123"));
                this._dataCollection.CompleteAdding();

                this._dispatcher.Process();
            }

            [Fact]
            public void Canceled_DoNothing_StopProperly()
            {
                var queryId = this._subscriptions.Keys.Single();

                this._dataCollection.Add(new MyData(queryId, "123"));

                this._cts.Cancel();

                this._dispatcher.Process();
            }
        }
    }
}
