using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyClient
{
    internal interface IMyConnectorFactory
    {
        IMyConnector Create(string[] uris, IConnectionEventFirer eventFirer);
    }

    internal interface IMyConnector
    {
        void Connect();
    }

    internal class MyConnector : IMyConnector
    {
        private class Factory : IMyConnectorFactory
        {
            public IMyConnector Create(string[] uris, IConnectionEventFirer eventFirer)
            {
                return new MyConnector(uris, eventFirer);
            }
        }

        public static readonly IMyConnectorFactory DefaultFactory = new Factory();

        private string[] _uris;
        private IConnectionEventFirer _eventFirer;

        public MyConnector(string[] uris, IConnectionEventFirer eventFirer)
        {
            this._uris = uris;
            this._eventFirer = eventFirer;
        }

        public void Connect()
        {
 
        }
    }
}
