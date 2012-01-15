using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MyDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            var driver = new MyDriverClient("jeffz://server:12345");

            try
            {
                driver.Connect();
                driver.AddQuery(1);
                driver.AddQuery(2);
                driver.AddQuery(3);
            }
            catch
            {
                driver.Close();
                Console.WriteLine("Error occurred when connect or add query.");
                Environment.Exit(1);
            }

            new Thread(() => ReceiveData(driver)).Start();
        }

        private static void ReceiveData(MyDriverClient driver)
        {
            try
            {
                while (true)
                {
                    var data = driver.Receive();
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
            catch (MyDriverException)
            {
                driver.Close();
                Console.WriteLine("Error occurred when receive data.");
            }
        }
    }
}
