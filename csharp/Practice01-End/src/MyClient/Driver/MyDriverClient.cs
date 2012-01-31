using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyDriver;

namespace MyClient.Driver
{
    internal interface IMyDriverClient : IDisposable
    {
        void Connect();

        void AddQuery(int queryId);

        void RemoveQuery(int queryId);

        MyData Receive();
    }

    internal interface IMyDriverClientFactory
    {
        IMyDriverClient Create(string uri);
    }

    internal class MyDriverClientWrapper : IMyDriverClient
    {
        private class Factory : IMyDriverClientFactory
        {
            public IMyDriverClient Create(string uri)
            {
                return new MyDriverClientWrapper(new MyDriverClient(uri));
            }
        }

        public static readonly IMyDriverClientFactory DefaultFactory = new Factory();

        private readonly MyDriverClient _client;

        public MyDriverClientWrapper(MyDriverClient client)
        {
            this._client = client;
        }

        public void Connect()
        {
            this._client.Connect();
        }

        public void AddQuery(int queryId)
        {
            this._client.AddQuery(queryId);
        }

        public void RemoveQuery(int queryId)
        {
            this._client.RemoveQuery(queryId);
        }

        public MyData Receive()
        {
            return this._client.Receive();
        }

        public void Dispose()
        {
            this._client.Dispose();
        }
    }
}
