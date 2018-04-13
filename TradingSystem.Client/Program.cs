using System;
using TradingSystem.Core;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TradingSystem.Client
{
    class Program
    {
        static int Total = 0;
        static int ThreadCount = 10;
        static int MessageCount = 1000;
        static DateTime start;
        static CountdownEvent ThreadsReady = new CountdownEvent(ThreadCount);
        static ManualResetEvent ProcessSignal = new ManualResetEvent(false);

        public static void Main(string[] args)
        {
            
            var threads = new List<Thread>();
            for (int i = 0; i < ThreadCount; i++)
            {
                var t = new Thread(Run);
                threads.Add(t);
                t.Start(i);
            }
            
            ThreadsReady.Wait();
            start = DateTime.UtcNow;
            ProcessSignal.Set();

            threads.ForEach(t => t.Join());
            Console.WriteLine(DateTime.UtcNow - start);
        }

        static void Run(object state)
        {
            var n = (int)state * 100;
            var c = new OrderBookClient("127.0.0.1", 23456, "localhost:9092");
            c.Connect();
            ThreadsReady.Signal();
            ProcessSignal.WaitOne();    

            for (int i = 0; i < MessageCount; i++)
            {
                c.Enter(new Order(OrderEntryType.Add, n, Side.Buy, 100, 100, 0, 0));
                c.Enter(new Order(OrderEntryType.Update, n, Side.Buy, 100, 50, 0, 0));
                c.Enter(new Order(OrderEntryType.Add, n + 1, Side.Sell, 100, 100, 0, 0));
                Interlocked.Add(ref Total, 3);

                if (Total % 100 == 0)
                {
                    Console.WriteLine($"Sent: {Total} of {ThreadCount * 1000 * 3} msg. TPS: {Total * 1.0 / (DateTime.UtcNow - start).TotalSeconds}");
                }
            }
        }
    }
}
