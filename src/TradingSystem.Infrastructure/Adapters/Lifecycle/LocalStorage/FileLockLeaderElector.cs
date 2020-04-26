using Microsoft.Extensions.Logging;
using System.IO;
using TradingSystem.Application.Lifecycle;
using TradingSystem.Infrastructure.Adapters.Settings;

namespace TradingSystem.Infrastructure.Adapters.Lifecycle.LocalStorage
{
    public class FileLockLeaderElector : LeaderElector
    {
        private readonly string _filePath;
        private FileStream _fs;

        public FileLockLeaderElector(ILogger<FileLockLeaderElector> logger, ILifecycleManager lifecycleManager, FileAdapterSettings settings)
            : base(logger, lifecycleManager)
        {
            _filePath = settings.LockFileName;
        }

        protected override bool HasLock
        {
            get
            {
                try
                {
                    if (_fs == null)
                        return false;

                    _fs.Write(new byte[1] { 1 });
                    _fs.Seek(-1, SeekOrigin.Current);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        protected override void GetLockSync()
        {
            _fs = new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }

    }
}
