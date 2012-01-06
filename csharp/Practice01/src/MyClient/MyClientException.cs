using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyClient
{
    public sealed class MyClientException : Exception
    {
        public MyClientException(string message)
            : base(message)
        { }
    }
}
