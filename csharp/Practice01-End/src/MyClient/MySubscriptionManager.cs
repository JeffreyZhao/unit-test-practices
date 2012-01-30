using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace MyClient
{
    internal interface IMySubscriptionManagerFactory
    {
        IMySubscriptionManager Create(string name);
    }

    internal interface IMySubscriptionManager : IDisposable
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
        private IDictionary<int, MySubscription> _subscriptions;

        internal MySubscriptionManager(string name, IDictionary<int, MySubscription> subscriptions)
        {
            this._name = name;
            this._subscriptions = subscriptions;
        }

        public MySubscriptionManager(string name)
            : this(name, new ConcurrentDictionary<int, MySubscription>())
        { }

        public void AddSubscription(MySubscription subscription)
        {
 
        }

        public void OnConnected(object sender, EventArgs args)
        {
            
        }

        public void OnDisconnected(object sender, EventArgs args)
        {
 
        }

        public void Dispose()
        {

        }
    }
}
