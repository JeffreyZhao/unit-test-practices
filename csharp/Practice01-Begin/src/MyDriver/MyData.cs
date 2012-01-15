using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDriver
{
    public sealed class MyData
    {
        public MyData(int queryId, string value)
        {
            this.QueryID = queryId;
            this.Value = value;
        }

        public int QueryID { get; private set; }

        public string Value { get; private set; }

        public override string ToString()
        {
            return this.QueryID + ", " + this.Value;
        }
    }
}
