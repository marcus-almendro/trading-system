using AutoMapper;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using TradingSystem.Application.Integration;
using TradingSystem.Application.Lifecycle;
using TradingSystem.Domain.Common;
using TradingSystem.Infrastructure.Adapters.Service.gRPC;
using TradingSystem.Infrastructure.Adapters.Settings;

namespace TradingSystem.Infrastructure.Adapters.Integration.LocalStorage
{
    public class FileEventDispatcher : EventDispatcher<IReadOnlyList<DomainEvent>, DomainEventCollection>
    {
        private readonly string _filePath;
        private readonly IMapper _mapper;
        private readonly ILogger<FileEventDispatcher> _logger;
        private FileStream _stream;

        public FileEventDispatcher(FileAdapterSettings settings, IMapper mapper, ILifecycleManager lifecycleManager, ILogger<FileEventDispatcher> logger)
            : base(lifecycleManager, logger)
        {
            _mapper = mapper;
            _filePath = settings.EventsFileName;
            _logger = logger;
        }

        protected override void BecomingLeader()
        {
            _logger.LogInformation("Become leader, creating file stream to {filepath}", _filePath);
            _stream = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        }
        protected override void Publish(DomainEventCollection msg)
        {
            lock (_stream)
            {
                _logger.LogDebug("Saving to disk {msg}", msg);
                msg.WriteDelimitedTo(_stream);
                _stream.Flush();
                _logger.LogDebug("Flushed to disk");
            }
        }

        public void Dispose() => _stream.Dispose();

        protected override DomainEventCollection Map(IReadOnlyList<DomainEvent> obj) => _mapper.Map<DomainEventCollection>(obj);
    }
}
