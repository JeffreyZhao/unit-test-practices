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

        internal Mock<MyConnection> _connectionMock;

        public MyConnectionTest()
        {
            this._name = "MyName";
            this._uris = new[] { "uri_0", "uri_1", "uri_2", "uri_3", "uri_4" };

            this._threadUtilsMock = new Mock<DelayThreadUtils> { CallBase = true };

            this._subscriptionManagerMock = new Mock<IMySubscriptionManager>(MockBehavior.Strict);
            this._subscriptionManagerFactoryMock = new Mock<IMySubscriptionManagerFactory>(MockBehavior.Strict);
            this._subscriptionManagerFactoryMock.Setup(f => f.Create(this._name)).Returns(this._subscriptionManagerMock.Object);
                 
            this._connectorMock = new Mock<IMyConnector>(MockBehavior.Strict);
            this._connectorFactoryMock = new Mock<IMyConnectorFactory>(MockBehavior.Strict);
            this._connectorFactoryMock.Setup(f => f.Create(this._uris, It.IsAny<IConnectionEventFirer>())).Returns(this._connectorMock.Object);

            this._connectionMock = new Mock<MyConnection>(
                this._name,
                this._uris,
                this._threadUtilsMock.Object,
                this._subscriptionManagerFactoryMock.Object,
                this._connectorFactoryMock.Object) { CallBase = true };
        }

        public class Open : MyConnectionTest
        {
            [Fact]
            public void Call_DelegateToConnectorInNewThread()
            {
                this._connectorMock.Setup(c => c.Connect());

                this._connectionMock.Object.Open();

                this._threadUtilsMock.Verify(tu => tu.StartNew("MyConnector_" + this._name, It.IsAny<ThreadStart>()), Times.Once());
                this._connectorMock.Verify(c => c.Connect(), Times.Never());

                this._threadUtilsMock.Object.Execute();
                this._connectorMock.Verify(c => c.Connect(), Times.Once());
            }
        }

        public class Connected : MyConnectionTest
        {
            [Fact]
            public void Fire_DelegateToSubscriptionManager()
            {
                this._subscriptionManagerMock.Setup(m => m.OnConnected(It.IsAny<MyConnection>(), EventArgs.Empty));

                ((IConnectionEventFirer)this._connectionMock.Object).FireConnected();

                this._subscriptionManagerMock.Verify(m => m.OnConnected(this._connectionMock.Object, EventArgs.Empty), Times.Once());
            }
        }

        public class Disconnected : MyConnectionTest
        {
            [Fact]
            public void Fire_DelegateToSubscriptionManager()
            {
                this._subscriptionManagerMock.Setup(m => m.OnDisconnected(It.IsAny<MyConnection>(), EventArgs.Empty));

                ((IConnectionEventFirer)this._connectionMock.Object).FireDisconnected();

                this._subscriptionManagerMock.Verify(m => m.OnDisconnected(this._connectionMock.Object, EventArgs.Empty), Times.Once());
            }
        }
    }
}
