using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Moq;
using MyClient.Tests.Threading;
using System.Threading;
using Xunit;

namespace MyClient.Tests
{
    public class MyConnectionTest
    {
        internal string _name;
        internal string[] _uris;

        internal Mock<DelayThreadUtils> _threadUtilsMock;

        internal Mock<IMySubscriptionManager> _subscriptionManagerMock;
        internal Mock<IMySubscriptionManagerFactory> _subscriptionManagerFactoryMock;

        internal Mock<IMyConnector> _connectorMock;
        internal Mock<IMyConnectorFactory> _connectorFactoryMock;

        internal MyConnection _connection;

        public MyConnectionTest()
        {
            this._name = "MyName";
            this._uris = new[] { "uri_0", "uri_1", "uri_2", "uri_3", "uri_4" };
                 
            this._connectorMock = new Mock<IMyConnector>(MockBehavior.Strict);
            this._connectorFactoryMock = new Mock<IMyConnectorFactory>(MockBehavior.Strict);
            this._connectorFactoryMock.Setup(f => f.Create(this._uris, It.IsAny<IConnectionEventFirer>())).Returns(this._connectorMock.Object);

            this._subscriptionManagerMock = new Mock<IMySubscriptionManager>(MockBehavior.Strict);
            this._subscriptionManagerFactoryMock = new Mock<IMySubscriptionManagerFactory>(MockBehavior.Strict);
            this._subscriptionManagerFactoryMock.Setup(f => f.Create(this._name, this._connectorMock.Object)).Returns(this._subscriptionManagerMock.Object);

            this._connection = new MyConnection(
                this._name,
                this._uris,
                this._connectorFactoryMock.Object,
                this._subscriptionManagerFactoryMock.Object);
        }

        public class Open : MyConnectionTest
        {
            [Fact]
            public void Call_DelegateToSubscriptionManager()
            {
                this._subscriptionManagerMock.Setup(sm => sm.StartConnecting());

                this._connection.Open();

                this._subscriptionManagerMock.Verify(sm => sm.StartConnecting(), Times.Once());
            }
        }

        public class Connected : MyConnectionTest
        {
            [Fact]
            public void Fire_DelegateToSubscriptionManager()
            {
                this._subscriptionManagerMock.Setup(m => m.OnConnected(It.IsAny<MyConnection>(), EventArgs.Empty));

                ((IConnectionEventFirer)this._connection).FireConnected();

                this._subscriptionManagerMock.Verify(m => m.OnConnected(this._connection, EventArgs.Empty), Times.Once());
            }
        }

        public class Disconnected : MyConnectionTest
        {
            [Fact]
            public void Fire_DelegateToSubscriptionManager()
            {
                this._subscriptionManagerMock.Setup(m => m.OnDisconnected(It.IsAny<MyConnection>(), EventArgs.Empty));

                ((IConnectionEventFirer)this._connection).FireDisconnected();

                this._subscriptionManagerMock.Verify(m => m.OnDisconnected(this._connection, EventArgs.Empty), Times.Once());
            }
        }

        public class Dispose : MyConnectionTest
        {
            [Fact]
            public void Call_UnregisterHandlersAndDisposeManager()
            {
                this._subscriptionManagerMock.Setup(m => m.Dispose());

                this._connection.Dispose();

                this._subscriptionManagerMock.Verify(m => m.Dispose(), Times.Once());

                ((IConnectionEventFirer)this._connection).FireConnected();
                ((IConnectionEventFirer)this._connection).FireDisconnected();
            }
        }
    }
}
