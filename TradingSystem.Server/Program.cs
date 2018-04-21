using System;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using TradingSystem.Core;

namespace TradingSystem.Server
{
    class Program
    {
        public static void Main(string[] args)
        {
            var svc = new OrderBookServer("0.0.0.0", 23456);
            svc.Connect();
            Task.Delay(-1);
        }
    }
}
