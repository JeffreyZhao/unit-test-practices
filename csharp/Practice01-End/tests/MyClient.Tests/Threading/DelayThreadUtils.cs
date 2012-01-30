using System;
using System.Threading;
using MyClient.Threading;

namespace MyClient.Tests.Threading
{
    public class DelayThreadUtils : IThreadUtils
    {
        private ThreadStart _start;

        public virtual void StartNew(string name, ThreadStart start)
        {
            this._start = start;
        }

        public void Sleep(int millisecondsTimeout) { }

        public void Execute()
        {
            if (this._start != null) this._start();
        }
    }
}
