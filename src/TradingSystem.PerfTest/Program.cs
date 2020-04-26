using Grpc.Net.Client;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using TradingSystem.Domain.Common;
using TradingSystem.Infrastructure.Adapters.Service.gRPC;

namespace TradingSystem.PerfTest
{
    internal class Program
    {
        private static readonly string Symbol = "test";
        private static int Total = 0;
        private static int ThreadCount;
        private static int IterationsPerThread;
        private static DateTime Start;
        private static CountdownEvent ThreadsReady;
        private static readonly ManualResetEvent ProcessSignal = new ManualResetEvent(false);
        private static string LeaderUrl, FollowerUrl;

        public static void Main(string[] args)
        {
            try
            {
                ThreadCount = Convert.ToInt32(args[0]);
                IterationsPerThread = Convert.ToInt32(args[1]);
                LeaderUrl = args[2];
                FollowerUrl = args[3];
            }
            catch
            {
                Console.WriteLine("TradingSystem.PerfTest ThreadCount IterationsPerThread LeaderUrl FollowerUrl");
                Process.GetCurrentProcess().Kill();
            }

            ThreadsReady = new CountdownEvent(ThreadCount);

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var threads = Enumerable.Range(1, ThreadCount).Select(i => (symbol: $"{Symbol}_{i}", thread: new Thread(Run))).ToList();

            threads.ForEach(p => p.thread.Start(p.symbol));

            ThreadsReady.Wait();
            Start = DateTime.UtcNow;
            Console.WriteLine($"{ThreadCount} thread waiting... Press ENTER to start!");
            Console.ReadLine();
            ProcessSignal.Set();

            threads.ForEach(p => p.thread.Join());
            Console.WriteLine(DateTime.UtcNow - Start);
        }

        private static void Run(object state)
        {
            var c = NewClient();
            var symbol = (string)state;

            if (c.AddOrderBook(new NewOrderBook { Symbol = symbol }).Value == ErrorCode.Success.Value)
                Console.WriteLine($"Created order book {symbol}");

            ThreadsReady.Signal();
            ProcessSignal.WaitOne();

            int currentOrderId = 0;
            for (int i = 0; i < IterationsPerThread; i++)
            {
                AddBuyOrder(c, symbol, ref currentOrderId);
                UpdateOrder(c, symbol, currentOrderId);
                AddSellOrder(c, symbol, ref currentOrderId);
                DeleteOrder(c, symbol, currentOrderId);

                Interlocked.Add(ref Total, 4);

                if (Total % 100 == 0)
                    Console.WriteLine($"Sent: {Total} of {ThreadCount * IterationsPerThread * 4} msgs. TPS: {Total * 1.0 / (DateTime.UtcNow - Start).TotalSeconds}");
            }

            AddBuyOrder(c, symbol, ref currentOrderId);

            var leaderEnd = DateTime.UtcNow;

            c = NewFollowerClient();
            long maxIdFromFollower;
            while ((maxIdFromFollower = TryGetMaxOrderId(c, symbol)) != currentOrderId)
            {
                Console.WriteLine($"Waiting sync for {symbol}, maxIdFromFollower: {maxIdFromFollower}");
                Thread.Sleep(100);
            }
            var followerEnd = DateTime.UtcNow;

            Console.WriteLine($"Sync time for {symbol}: {followerEnd - leaderEnd}");
        }

        private static void AddBuyOrder(OrderBookServiceGrpc.OrderBookServiceGrpcClient c, string symbol, ref int currentOrderId)
        {
            var r = c.AddBuyOrder(new OrderMsg() { Symbol = symbol, Type = OrderType.Buy, Price = 100, Size = 100, TraderId = 0 }).Value;
            if (r != ErrorCode.Success.Value) Console.WriteLine($"Error when adding buy order to symbol {symbol}, error code: {r}");
            currentOrderId++;
        }

        private static void UpdateOrder(OrderBookServiceGrpc.OrderBookServiceGrpcClient c, string symbol, int currentOrderId)
        {
            var r = c.UpdateOrder(new OrderMsg() { Symbol = symbol, Type = OrderType.Buy, Id = currentOrderId, Price = 100, Size = 50, TraderId = 0 }).Value;
            if (r != ErrorCode.Success.Value) Console.WriteLine($"Error when updating order to symbol {symbol}, error code: {r}");
        }

        private static void AddSellOrder(OrderBookServiceGrpc.OrderBookServiceGrpcClient c, string symbol, ref int currentOrderId)
        {
            var r = c.AddSellOrder(new OrderMsg() { Symbol = symbol, Type = OrderType.Sell, Price = 100, Size = 100, TraderId = 0 }).Value;
            if (r != ErrorCode.Success.Value) Console.WriteLine($"Error when adding sell order to symbol {symbol}, error code: {r}");
            currentOrderId++;
        }

        private static void DeleteOrder(OrderBookServiceGrpc.OrderBookServiceGrpcClient c, string symbol, int currentOrderId)
        {
            var r = c.DeleteOrder(new OrderMsg() { Symbol = symbol, Type = OrderType.Sell, Id = currentOrderId, TraderId = 0 }).Value;
            if (r != ErrorCode.Success.Value) Console.WriteLine($"Error when deleting order of symbol {symbol}, error code: {r}");
        }

        private static long TryGetMaxOrderId(OrderBookServiceGrpc.OrderBookServiceGrpcClient c, string symbol)
        {
            var orders = c.Status(new GetStatus() { Symbol = symbol, IncludeDump = true }).AllOrders;
            return orders.Count > 0 ? orders[0].Id : 0;
        }

        private static OrderBookServiceGrpc.OrderBookServiceGrpcClient NewClient() =>
            new OrderBookServiceGrpc.OrderBookServiceGrpcClient(GrpcChannel.ForAddress(LeaderUrl));

        private static OrderBookServiceGrpc.OrderBookServiceGrpcClient NewFollowerClient() =>
            new OrderBookServiceGrpc.OrderBookServiceGrpcClient(GrpcChannel.ForAddress(FollowerUrl));
    }
}