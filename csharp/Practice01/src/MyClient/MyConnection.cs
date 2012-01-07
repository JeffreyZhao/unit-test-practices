using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyWork
{
    public class MyConnection
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

        public void Close()
        {
            throw new NotImplementedException();
        }

        public IDisposable Subscribe(int queryId, IMySubscriber subscriber)
        {
            throw new NotImplementedException();
        }

        public event EventHandler Connected;

        public event EventHandler ConnectionFailed;

        public event EventHandler Disconnected;
    }
}
