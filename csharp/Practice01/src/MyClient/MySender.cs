using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyClient
{
    public sealed class MySender 
    {
        private readonly Random _random = new Random(DateTime.Now.Millisecond);

        private readonly MyReceiver _receiver = new MyReceiver();

        public MySender(string uri) { }

        public MyReceiver Receiver { get { return this._receiver; } }

        public void AddQuery(int queryId)
        {
            if (this._random.NextDouble() < 0.05)
            {
                throw new MyClientException("Error occurred when add query.");
            }

            this._receiver.AddQuery(queryId);
        }

        public void RemoveQuery(int queryId)
        {
            if (this._random.NextDouble() < 0.05)
            {
                throw new MyClientException("Error occurred when remove query.");
            }

            this._receiver.RemoveQuery(queryId);
        }

        public void Close()
        {
            this._receiver.Close();
        }
    }
}
