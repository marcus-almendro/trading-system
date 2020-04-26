using AutoMapper;
using FluentAssertions;
using Grpc.Net.Client;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TradingSystem.Application.DTO.Commands;
using TradingSystem.Domain.Orders;
using TradingSystem.Infrastructure.Adapters.Service.gRPC;
using TradingSystem.Infrastructure.Serialization.AutoMapper;
using Xunit;
using OrderType = TradingSystem.Domain.Orders.OrderType;

namespace TradingSystem.Tests.Integration
{
    public abstract class FailoverIntegrationTest : IDisposable
    {
        private readonly string _symbol;
        private readonly GetStatus _statusRequest, _dumpRequest;
        private readonly IMapper _mapper;
        private readonly ErrorCodeMsg _success;
        private Process _leader, _follower;
        private OrderBookServiceGrpc.OrderBookServiceGrpcClient _leaderClient, _followerClient;

        public FailoverIntegrationTest()
        {
            _symbol = "test";
            _statusRequest = new GetStatus() { Symbol = "test" };
            _dumpRequest = new GetStatus() { Symbol = "test", IncludeDump = true };
            _mapper = new MapperConfiguration(cfg => cfg.AddProfile<InfraMapperProfile>()).CreateMapper();
            _success = new ErrorCodeMsg() { Value = 1, Description = "Success" };

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            Setup();

            _leader = CreateService("5000", out _leaderClient, true);
            _follower = CreateService("5001", out _followerClient, false);
        }

        public virtual void Dispose()
        {
            if (!_leader.HasExited) _leader.Kill();
            if (!_follower.HasExited) _follower.Kill();
        }

        public abstract string StorageType { get; }
        public abstract string LockStrategy { get; }
        protected abstract void Setup();

        private Process CreateService(string port, out OrderBookServiceGrpc.OrderBookServiceGrpcClient client, bool shouldBeLeader)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = "dotnet",
                    Arguments = "../../../../TradingSystem.Server/bin/Debug/netcoreapp3.1/TradingSystem.Server.dll",
                },
            };
            process.StartInfo.EnvironmentVariables.Add("GrpcAdapter__Port", port);
            process.StartInfo.EnvironmentVariables.Add("Main__Storage", StorageType);
            process.StartInfo.EnvironmentVariables.Add("Main__LockStrategy", LockStrategy);
            process.StartInfo.EnvironmentVariables.Add("Main__Debug", Debugger.IsAttached.ToString());
            process.OutputDataReceived += (s, e) => Debug.WriteLine(e.Data);
            process.ErrorDataReceived += (s, e) => Debug.WriteLine(e.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            var c = new OrderBookServiceGrpc.OrderBookServiceGrpcClient(GrpcChannel.ForAddress("http://localhost:" + port));
            WaitUntilSucceed(() =>
            {
                var status = c.Status(_statusRequest);
                status.LastStatusChange.Should().NotBe(default(DateTimeOffset).UtcTicks);
                status.IsLeader.Should().Be(shouldBeLeader);
            });
            client = c;
            return process;
        }

        private void TryAddRemoteOrderBook(OrderBookServiceGrpc.OrderBookServiceGrpcClient client)
        {
            for (var i = 0; i < 100; i++)
            {
                try
                {
                    client.AddOrderBook(new NewOrderBook { Symbol = _symbol });
                    return;
                }
                catch
                {
                    Task.Delay(200).Wait();
                }
            }
            throw new Exception("not added");
        }

        private void WaitUntilSucceed(Action action)
        {
            for (var i = 0; i < 300; i++)
            {
                try
                {
                    action();
                    return;
                }
                catch
                {
                    Task.Delay(200).Wait();
                }
            }

            throw new Exception("action failed");
        }

        [Fact]
        public void ShouldReplicateEvents()
        {
            TryAddRemoteOrderBook(_leaderClient);

            _leaderClient.AddBuyOrder(_mapper.Map<OrderMsg>(new BuyOrderCommand() { Price = 100, Size = 100, Symbol = _symbol, TraderId = 0 })).Should().BeEquivalentTo(_success);

            WaitUntilSucceed(() =>
            {
                _followerClient.Status(_statusRequest).TotalOrders.Should().Be(1);

                var status = _followerClient.Status(_dumpRequest);
                status.AllOrders.Count.Should().Be(1);
                status.AllOrders[0].Should().BeEquivalentTo(_mapper.Map<OrderMsg>(new Order(_symbol, 1, OrderType.Buy, 100, 100, 0)));
            });
        }

        [Fact]
        public void Standalone()
        {
            _follower.Kill();

            TryAddRemoteOrderBook(_leaderClient);

            _leaderClient.AddBuyOrder(_mapper.Map<OrderMsg>(new BuyOrderCommand() { Price = 100, Size = 100, Symbol = _symbol, TraderId = 0 })).Should().BeEquivalentTo(_success);
            _leader.Kill();
            _leader = CreateService("5000", out _leaderClient, true);

            WaitUntilSucceed(() =>
            {
                _leaderClient.Status(_statusRequest).TotalOrders.Should().Be(1);
            });
        }

        [Fact]
        public void ShouldFailover()
        {
            TryAddRemoteOrderBook(_leaderClient);

            _leaderClient.AddBuyOrder(_mapper.Map<OrderMsg>(new BuyOrderCommand() { Price = 100, Size = 100, Symbol = _symbol, TraderId = 0 })).Should().BeEquivalentTo(_success);

            WaitUntilSucceed(() =>
            {
                _followerClient.Status(_statusRequest).TotalOrders.Should().Be(1);

                var status = _followerClient.Status(_dumpRequest);
                status.AllOrders.Count.Should().Be(1);
                status.AllOrders[0].Should().BeEquivalentTo(_mapper.Map<OrderMsg>(new Order(_symbol, 1, OrderType.Buy, 100, 100, 0)));
            });

            _leader.Kill();

            WaitUntilSucceed(() => _followerClient.Status(_statusRequest).IsLeader.Should().BeTrue());
        }

        [Fact]
        public void ShouldFailoverMultipleTimes()
        {
            TryAddRemoteOrderBook(_leaderClient);

            _leaderClient.AddBuyOrder(_mapper.Map<OrderMsg>(new BuyOrderCommand() { Price = 100, Size = 100, Symbol = _symbol, TraderId = 0 })).Should().BeEquivalentTo(_success);

            WaitUntilSucceed(() =>
            {
                _followerClient.Status(_statusRequest).TotalOrders.Should().Be(1);

                var status = _followerClient.Status(_dumpRequest);
                status.AllOrders.Count.Should().Be(1);
                status.AllOrders[0].Should().BeEquivalentTo(_mapper.Map<OrderMsg>(new Order(_symbol, 1, OrderType.Buy, 100, 100, 0)));
            });

            //first failover
            _leader.Kill();

            WaitUntilSucceed(() => _followerClient.Status(_statusRequest).IsLeader.Should().BeTrue());

            _leader = CreateService("5000", out _leaderClient, false);

            WaitUntilSucceed(() => _leaderClient.Status(_statusRequest).TotalOrders.Should().Be(1));

            _followerClient.AddBuyOrder(_mapper.Map<OrderMsg>(new BuyOrderCommand() { Price = 100, Size = 100, Symbol = _symbol, TraderId = 0 })).Should().BeEquivalentTo(_success);

            WaitUntilSucceed(() =>
            {
                _leaderClient.Status(_statusRequest).TotalOrders.Should().Be(2);

                var status = _leaderClient.Status(_dumpRequest);
                status.AllOrders.Count.Should().Be(2);
                status.AllOrders[0].Should().BeEquivalentTo(_mapper.Map<OrderMsg>(new Order(_symbol, 1, OrderType.Buy, 100, 100, 0)));
                status.AllOrders[1].Should().BeEquivalentTo(_mapper.Map<OrderMsg>(new Order(_symbol, 2, OrderType.Buy, 100, 100, 0)));
            });

            //second failover
            _follower.Kill();

            WaitUntilSucceed(() => _leaderClient.Status(_statusRequest).IsLeader.Should().BeTrue());

            _follower = CreateService("5001", out _followerClient, false);

            _leaderClient.AddBuyOrder(_mapper.Map<OrderMsg>(new BuyOrderCommand() { Price = 100, Size = 100, Symbol = _symbol, TraderId = 0 })).Should().BeEquivalentTo(_success);

            WaitUntilSucceed(() =>
            {
                _followerClient.Status(_statusRequest).TotalOrders.Should().Be(3);

                var status = _followerClient.Status(_dumpRequest);
                status.AllOrders.Count.Should().Be(3);
                status.AllOrders[0].Should().BeEquivalentTo(_mapper.Map<OrderMsg>(new Order(_symbol, 1, OrderType.Buy, 100, 100, 0)));
                status.AllOrders[1].Should().BeEquivalentTo(_mapper.Map<OrderMsg>(new Order(_symbol, 2, OrderType.Buy, 100, 100, 0)));
                status.AllOrders[2].Should().BeEquivalentTo(_mapper.Map<OrderMsg>(new Order(_symbol, 3, OrderType.Buy, 100, 100, 0)));
            });
        }
    }
}
