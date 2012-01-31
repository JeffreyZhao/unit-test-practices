using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using MyClient.Threading;
using MyClient.Driver;
using Xunit;
using System.Linq.Expressions;
using MyDriver;

namespace MyClient.Tests
{
    public class MyConnectorTest
    {
        internal string[] _uris;

        internal Mock<IConnectionEventFirer> _eventFirerMock;
        internal Mock<IThreadUtils> _threadUtilsMock;
        internal Mock<IMyDriverClientFactory> _clientFactoryMock;

        internal MyConnector _connector;

        public MyConnectorTest()
        {
            this._uris = new[] { "uri_0", "uri_1", "uri_2", "uri_3", "uri_4" };
            this._eventFirerMock = new Mock<IConnectionEventFirer>(MockBehavior.Strict);
            this._threadUtilsMock = new Mock<IThreadUtils>(MockBehavior.Strict);
            this._clientFactoryMock = new Mock<IMyDriverClientFactory>(MockBehavior.Strict);

            this._connector = new MyConnector(
                this._uris,
                this._eventFirerMock.Object,
                this._threadUtilsMock.Object,
                this._clientFactoryMock.Object);
        }

        public class Connect : MyConnectorTest
        {
            [Fact]
            public void SucceededDirectly_ConnectCalled_ClientGot_EventFired()
            {
                var clientMock = new Mock<IMyDriverClient>(MockBehavior.Strict);
                clientMock.Setup(c => c.Connect());

                this._clientFactoryMock.Setup(c => c.Create(this._uris[0])).Returns(clientMock.Object);

                this._eventFirerMock.Setup(f => f.FireConnected());

                this._connector.Connect();

                clientMock.Verify(c => c.Connect(), Times.Once());
                Assert.Same(clientMock.Object, this._connector.Client);

                this._eventFirerMock.Verify(f => f.FireConnected(), Times.Once());
            }

            [Fact]
            public void FailedAndSucceededFinally_MultileClientsCreated_ClientGot()
            {
                var validIndex = 1;

                var failedClientMock = new Mock<IMyDriverClient>(MockBehavior.Strict);
                failedClientMock.Setup(c => c.Connect()).Throws<Exception>();

                var clientMock = new Mock<IMyDriverClient>(MockBehavior.Strict);
                clientMock.Setup(c => c.Connect());

                for (var i = 0; i < this._uris.Length; i++)
                {
                    if (i == validIndex) // failed and succeeded
                    {
                        this._clientFactoryMock.SetupSequence(f => f.Create(this._uris[i]))
                            .Returns(failedClientMock.Object)
                            .Returns(clientMock.Object);
                    }
                    else if (i % 2 == 0) // factory always failed
                    {
                        this._clientFactoryMock.Setup(f => f.Create(this._uris[i])).Throws(new MyDriverException("Hello World"));
                    }
                    else // Connect always failed
                    {
                        this._clientFactoryMock.Setup(f => f.Create(this._uris[i])).Returns(failedClientMock.Object);
                    }
                }

                this._threadUtilsMock.Setup(tu => tu.Sleep(MyConnector.ReconnectInterval));

                this._eventFirerMock.Setup(f => f.FireConnectFailed());
                this._eventFirerMock.Setup(f => f.FireConnected());

                this._connector.Connect();

                Assert.Same(clientMock.Object, this._connector.Client);
                clientMock.Verify(c => c.Connect(), Times.Once());

                this._threadUtilsMock.Verify(tu => tu.Sleep(MyConnector.ReconnectInterval), Times.Exactly(this._uris.Length + validIndex));

                this._eventFirerMock.Verify(f => f.FireConnectFailed(), Times.Exactly(this._uris.Length + validIndex));
                this._eventFirerMock.Verify(f => f.FireConnected(), Times.Once());
            }
        }

        public class CloseClient : MyConnectorTest
        {
            [Fact]
            public void Call_ClientIsNull_ClientDisposed()
            {
                var clientMock = new Mock<IMyDriverClient>(MockBehavior.Strict);
                clientMock.Setup(c => c.Connect());
                clientMock.Setup(c => c.Dispose());

                this._clientFactoryMock.Setup(f => f.Create(this._uris[0])).Returns(clientMock.Object);

                this._eventFirerMock.Setup(f => f.FireConnected());
                this._eventFirerMock.Setup(f => f.FireDisconnected());

                this._connector.Connect();

                this._connector.CloseClient();

                Assert.Null(this._connector.Client);
                clientMock.Verify(c => c.Dispose(), Times.Once());

                this._eventFirerMock.Verify(f => f.FireDisconnected(), Times.Once());
            }
        }
    }
}
