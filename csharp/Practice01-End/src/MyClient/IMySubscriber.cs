using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyClient
{
    public interface IMySubscriber
    {
        void OnBegin();
        void OnMessage(string message);
    }
}
