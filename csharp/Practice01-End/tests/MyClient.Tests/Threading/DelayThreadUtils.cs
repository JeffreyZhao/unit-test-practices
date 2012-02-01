using System;
using System.Threading;
using MyClient.Threading;
using System.Collections.Generic;

namespace MyClient.Tests.Threading
{
    public class DelayThreadUtils : IThreadUtils
    {
        private List<Action> _actionsToExecute = new List<Action>();

        public virtual void StartNew(string name, ThreadStart start)
        {
            this._actionsToExecute.Add(() => start());
        }

        public void Sleep(int millisecondsTimeout) { }

        public void Execute()
        {
            foreach (var action in this._actionsToExecute) action();
        }
    }
}
