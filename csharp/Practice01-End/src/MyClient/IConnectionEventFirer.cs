using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyClient
{
    internal interface IConnectionEventFirer
    {
        void FireConnected();
        void FireConnectFailed();
        void FireDisconnected();
    }
}
