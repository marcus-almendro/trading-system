using Confluent.Kafka;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradingSystem.Infrastructure.Adapters.Settings;
using TradingSystem.Infrastructure.ReadinessProbe;

namespace TradingSystem.Tests.Utils
{
    public static class DockerUtils
    {
        private static CreateContainerResponse _zookeeperContainer, _kafkaContainer, _consulContainer;
        private static DockerClient _client;
        private static NetworkingConfig _networkConfig;

        //prerequisite: pull docker images to local machine
        public static void StartDockerContainers()
        {
            _client = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();
            CleanupOld();
            CreateNetwork();
            StartContainers();
        }

        private static void CreateNetwork()
        {
            _client.Networks.CreateNetworkAsync(new NetworksCreateParameters
            {
                Name = "containers_net",
            }).Wait();

            _networkConfig = new NetworkingConfig
            {
                EndpointsConfig = new Dictionary<string, EndpointSettings>
                    {
                        { "containers_net", new EndpointSettings { Links = new [] { "zk", "kafka" } } }
                    }
            };
        }

        private static void StartContainers()
        {
            _zookeeperContainer = _client.Containers.CreateContainerAsync(new CreateContainerParameters()
            {
                Image = "confluentinc/cp-zookeeper:latest",
                Env = new[] { "ZOOKEEPER_CLIENT_PORT=2181" },
                Name = "zk",
                Hostname = "zk",
                NetworkingConfig = _networkConfig
            }).Result;
            _kafkaContainer = _client.Containers.CreateContainerAsync(new CreateContainerParameters()
            {

                Image = "confluentinc/cp-kafka:latest",
                Env = new[] {
                    "KAFKA_ZOOKEEPER_CONNECT=zk:2181",
                    "KAFKA_LISTENERS=PLAINTEXT://:9092",
                    "KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://localhost:9092",
                    "KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR=1",
                    "KAFKA_AUTO_CREATE_TOPICS_ENABLE=false"
                },
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        { "9092/tcp", new [] { new PortBinding { HostIP = "127.0.0.1", HostPort = "9092" } } }
                    }
                },
                NetworkingConfig = _networkConfig,
                Name = "kafka",
                Hostname = "kafka"
            }).Result;
            _consulContainer = _client.Containers.CreateContainerAsync(new CreateContainerParameters()
            {

                Image = "consul:latest",
                Env = new[] {
                    "CONSUL_BIND_INTERFACE=eth0",
                },
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        { "8500/tcp", new [] { new PortBinding { HostIP = "127.0.0.1", HostPort = "8500" } } }
                    }
                },
                NetworkingConfig = _networkConfig,
                Name = "consul",
                Hostname = "consul"
            }).Result;

            _client.Containers.StartContainerAsync(_zookeeperContainer.ID, null).Wait();
            _client.Containers.StartContainerAsync(_kafkaContainer.ID, null).Wait();
            _client.Containers.StartContainerAsync(_consulContainer.ID, null).Wait();
        }

        public static void WaitTopicCreation(KafkaAdapterSettings settings)
        {
            Task.Delay(10 * 1000).Wait();

            var resp = _client.Containers.ExecCreateContainerAsync(_kafkaContainer.ID, new ContainerExecCreateParameters
            {
                Cmd = new[] { "sh", "-c", $"kafka-topics --create --bootstrap-server localhost:9092 --replication-factor 1 --partitions 1 --topic {settings.EventsTopic}" }
            }).Result;
            _client.Containers.StartContainerExecAsync(resp.ID).Wait();

            var probe = new KafkaReadinessProbe(settings, new NullLogger<KafkaReadinessProbe>());

            Task.Delay(2000).Wait();

            if (!probe.IsReady)
                Task.Delay(2000).Wait();

            if (!probe.IsReady)
                throw new Exception("Cannot create topic");
        }

        public static void PutMessage<TKey, TValue>(string brokerList, string topic, TKey key, TValue value)
        {
            var conf = new ProducerConfig
            {
                BootstrapServers = brokerList,
                EnableIdempotence = true
            };
            using (var producer = new ProducerBuilder<TKey, TValue>(conf).Build())
                producer.ProduceAsync(topic, new Message<TKey, TValue>() { Key = key, Value = value }).Wait();

        }

        public static List<ConsumeResult<TKey, TValue>> GetMessages<TKey, TValue>(string brokerList, string topic, int count)
        {
            var conf = new ConsumerConfig
            {
                GroupId = "test-consumer-group",
                BootstrapServers = brokerList,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            using (var c = new ConsumerBuilder<TKey, TValue>(conf).Build())
            {
                c.Subscribe(topic);

                var list = new List<ConsumeResult<TKey, TValue>>();
                for (var i = 0; i < count; i++)
                {
                    var cr = c.Consume(TimeSpan.FromSeconds(30));
                    list.Add(cr);
                }
                return list;
            }
        }

        public static void CleanupCurrent()
        {
            CleanupDocker(_kafkaContainer.ID, _zookeeperContainer.ID, _consulContainer.ID);
        }

        private static void CleanupOld()
        {
            _client.Containers.PruneContainersAsync().Wait();
            _client.Networks.PruneNetworksAsync().Wait();
            var oldContainers = _client.Containers.ListContainersAsync(new ContainersListParameters() { All = true }).Result;
            CleanupDocker(oldContainers.Select(c => c.ID).ToArray());
        }

        private static void CleanupDocker(params string[] ids)
        {
            foreach (var id in ids)
                _client.Containers.KillContainerAsync(id, new ContainerKillParameters { Signal = "9" }).Wait();
            _client.Containers.PruneContainersAsync().Wait();
            _client.Networks.PruneNetworksAsync().Wait();
        }
    }
}
