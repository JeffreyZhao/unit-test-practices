using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MyClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var sender = new MySender("jeffz://server:12345");

            try
            {
                sender.AddQuery(1);
                sender.AddQuery(2);
                sender.AddQuery(3);
            }
            catch
            {
                Console.WriteLine("Error occurred when add query.");
                Environment.Exit(1);
            }

            new Thread(() => ReceiveData(sender.Receiver)).Start();
        }

        private static void ReceiveData(MyReceiver receiver)
        {
            try
            {
                while (true)
                {
                    var data = receiver.Receive();
                    if (data == null)
                    {
                        Console.WriteLine("Closed");
                        break;
                    }
                    else
                    {
                        Console.WriteLine(data);
                    }
                }
            }
            catch (MyClientException)
            {
                Console.WriteLine("Error occurred when receive data.");
            }
        }
    }
}
