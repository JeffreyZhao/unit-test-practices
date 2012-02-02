using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyClient
{
    public class MyConnection : IDisposable
    {
        public readonly static int ReconnectInterval = 3000;

        public MyConnection(string[] uris)
        {
            throw new NotImplementedException();
        }

        public void Open()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public int Subscribe(IMySubscriber subscriber)
        {
            throw new NotImplementedException();
        }

        public bool Unsubscribe(int subscriptionId)
        {
            throw new NotImplementedException();
        }

        public event EventHandler Connected;

        public event EventHandler ConnectFailed;

        public event EventHandler Disconnected;
    }
}
