using AutoMapper;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using TradingSystem.Application.Integration;
using TradingSystem.Application.Lifecycle;
using TradingSystem.Infrastructure.Adapters.Service.gRPC;
using TradingSystem.Infrastructure.Adapters.Settings;

namespace TradingSystem.Infrastructure.Adapters.Integration.LocalStorage
{
    public class FileEventReceiver<TDestination> : EventReceiver<DomainEventCollection, TDestination>
    {
        private readonly string _filePath;
        private readonly IMapper _mapper;
        private readonly ILogger<FileEventReceiver<TDestination>> _logger;
        private FileStream _stream;

        public FileEventReceiver(FileAdapterSettings settings, IMapper mapper, ILifecycleManager lifecycleManager, ILogger<FileEventReceiver<TDestination>> logger)
            : base(lifecycleManager, logger)
        {
            _mapper = mapper;
            _filePath = settings.EventsFileName;
            _logger = logger;
        }

        protected override void BeginFollowing()
        {
            _logger.LogInformation("Begin follow, creating file stream to {filepath}", _filePath);
            _stream = new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
        }

        protected override DomainEventCollection ConsumeNextMessage()
        {
            var col = _stream.Position < _stream.Length ? DomainEventCollection.Parser.ParseDelimitedFrom(_stream) : null;
            if (col != null)
                col.Offset = _stream.Position;
            return col;
        }

        protected override void WaitConsumptionEnd(long maxOffset)
        {
            while (_stream.Position != _stream.Length && IsRunning)
            {
                if (_stream.Position > maxOffset && maxOffset != -1)
                    break;

                Task.Delay(100).Wait();
            }
        }

        protected override TDestination Map(DomainEventCollection obj) => _mapper.Map<TDestination>(obj);
    }
}
