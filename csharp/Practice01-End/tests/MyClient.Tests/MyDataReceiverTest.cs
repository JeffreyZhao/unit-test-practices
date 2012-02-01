using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyClient.Driver;
using Moq;
using Xunit;
using MyDriver;

namespace MyClient.Tests
{
    public class MyDataReceiverTest
    {
        internal Mock<IMyDriverClient> _clientMock;
        internal Mock<IMyConnector> _connectorMock;
        internal MyDataReceiver _receiver;

        public MyDataReceiverTest()
        {
            this._clientMock = new Mock<IMyDriverClient>(MockBehavior.Strict);

            this._connectorMock = new Mock<IMyConnector>(MockBehavior.Strict);
            this._connectorMock.Setup(c => c.Client).Returns(this._clientMock.Object);

            this._receiver = new MyDataReceiver(this._connectorMock.Object);
        }

        public class Process : MyDataReceiverTest
        {
            [Fact]
            public void DataReceived_PutIntoCollection()
            {
                var dataList = Enumerable.Range(0, 10).Select(i => new MyData(0, i.ToString())).ToList();

                var setup = this._clientMock.SetupSequence(c => c.Receive());
                foreach (var data in dataList)
                {
                    setup = setup.Returns(data);
                }

                setup.Throws(new DummyException());

                Assert.Throws(typeof(DummyException), () => this._receiver.Process());

                Assert.False(this._receiver.CancellationToken.IsCancellationRequested);

                var producedList = this._receiver.DataProduced.ToList();
                Assert.Equal(dataList.Count, producedList.Count);
                for (var i = 0; i < dataList.Count; i++)
                {
                    Assert.Same(dataList[i], producedList[i]);
                }
            }

            [Fact]
            public void ErrorOccurred_TokenCanceled_CloseClient()
            {
                this._connectorMock.Setup(c => c.CloseClient());

                this._clientMock.SetupSequence(c => c.Receive()).Returns(new MyData(0, "")).Throws(new MyDriverException("Test"));

                this._receiver.Process();

                Assert.True(this._receiver.CancellationToken.IsCancellationRequested);

                Assert.Equal(1, this._receiver.DataProduced.Count);
                Assert.False(this._receiver.DataProduced.IsAddingCompleted);

                this._connectorMock.Verify(c => c.CloseClient(), Times.Once());
            }

            [Fact]
            public void NullReceived_TokenCanceled()
            {
                this._clientMock.SetupSequence(c => c.Receive()).Returns(new MyData(0, "")).Returns(null);

                this._receiver.Process();

                Assert.True(this._receiver.CancellationToken.IsCancellationRequested);

                Assert.Equal(1, this._receiver.DataProduced.Count);
                Assert.False(this._receiver.DataProduced.IsAddingCompleted);
            }
        }
    }
}
