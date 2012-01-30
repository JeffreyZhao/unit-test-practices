using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MyClient
{
    internal class MySubscription
    {
        private static int _seed = 0;

        public MySubscription(IMySubscriber subscriber)
        {
            this.QueryID = Interlocked.Increment(ref _seed);
            this.Subscriber = subscriber;
        }

        public IMySubscriber Subscriber { get; private set; }

        public int QueryID { get; private set; }
    }
}
