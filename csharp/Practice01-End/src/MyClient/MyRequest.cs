using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyClient
{
    internal enum MyRequestType
    {
        Subscribe,
        Unsubscribe
    }

    internal class MyRequest
    {
        public MyRequest(MyRequestType type, MySubscription subscription)
        {
            this.Type = type;
            this.Subscription = subscription;
        }

        public MyRequestType Type { get; private set; }

        public MySubscription Subscription { get; private set; }
    }
}
