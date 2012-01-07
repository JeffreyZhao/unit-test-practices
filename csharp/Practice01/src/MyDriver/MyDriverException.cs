using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDriver
{
    public sealed class MyDriverException : Exception
    {
        public MyDriverException(string message)
            : base(message)
        { }
    }
}
