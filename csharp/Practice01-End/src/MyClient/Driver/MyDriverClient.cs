using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyDriver;

namespace MyClient.Driver
{
    internal interface IMyDriverClient
    {
        void Connect();
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
    }
}
