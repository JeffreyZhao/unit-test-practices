using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MyClient.Threading
{
    internal interface IThreadUtils
    {
        void StartNew(string name, ThreadStart start);
    }

    internal class ThreadUtils : IThreadUtils
    {
        public static readonly ThreadUtils Instance = new ThreadUtils();

        public void StartNew(string name, ThreadStart start)
        {
            var thread = new Thread(start);
            thread.Name = name;
            thread.Start();
        }
    }
}
