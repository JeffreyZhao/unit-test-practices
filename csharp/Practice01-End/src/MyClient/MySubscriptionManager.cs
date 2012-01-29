using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyClient
{
    internal interface IMySubscriptionManagerFactory
    {
        IMySubscriptionManager Create(string name);
    }

    internal interface IMySubscriptionManager
    {
        void OnConnected(object sender, EventArgs args);
        void OnDisconnected(object sender, EventArgs args);
    }

    internal class MySubscriptionManager : IMySubscriptionManager
    {
        private class Factory : IMySubscriptionManagerFactory
        {
            public IMySubscriptionManager Create(string name)
            {
                return new MySubscriptionManager(name);
            }
        }

        public static readonly IMySubscriptionManagerFactory DefaultFactory = new Factory();

        private string _name;

        public MySubscriptionManager(string name)
        {
            this._name = name;
        }

        public void OnConnected(object sender, EventArgs args)
        {
            
        }

        public void OnDisconnected(object sender, EventArgs args)
        {
 
        }
    }
}
