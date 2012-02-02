using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MyClient
{
    public class Program
    {
        private class TestSubscriber : IMySubscriber
        {
            public TestSubscriber(string name)
            {
                this.Name = name;
            }

            public string Name { get; private set; }

            public void OnBegin()
            {
                Console.WriteLine(this.Name + " - begin");
            }

            public void OnMessage(string message)
            {
                Console.WriteLine(this.Name + " - " + message);
            }
        }

        public static void Main(string[] args)
        {
            var appender = new log4net.Appender.ConsoleAppender();
            appender.Layout = new log4net.Layout.SimpleLayout();
            appender.AddFilter(new log4net.Filter.LevelRangeFilter { LevelMin = log4net.Core.Level.Info });
            log4net.Config.BasicConfigurator.Configure(appender);

            using (var connection = new MyConnection(new[] { "uri_0", "uri_1" }))
            {
                connection.Open();

                Console.WriteLine("Press enter to subscribe data and press again to exit.");
                Console.ReadLine();

                connection.Subscribe(new TestSubscriber("A"));
                connection.Subscribe(new TestSubscriber("B"));

                Console.ReadLine();
            }
        }
    }
}
