namespace DeafTelephone.Web.Services.Services.Infrastructure
{
    using DeafTelephone.Web.Core.Services.Infrastructure;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.IO;

    internal sealed class FileWatcher : IFileWatcher, IDisposable
    {
        private readonly Dictionary<string, FileSystemWatcher> _watchers = new();
        private readonly ILogger<FileWatcher> _logger;

        public FileWatcher(ILogger<FileWatcher> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WatchForFile(string fileName, Action onChanged)
        {
            try
            {
                var newWatcher = new FileSystemWatcher(Path.GetDirectoryName(fileName), Path.GetFileName(fileName))
                {
                    EnableRaisingEvents = true,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
                };
                _watchers.Add(fileName, newWatcher);
                newWatcher.Changed += (_, e) => onChanged();

                _logger.LogInformation($"{nameof(FileWatcher)}: start to watch on {fileName}!");
            }
            catch (Exception es)
            {
                _logger.LogError($"{nameof(FileWatcher)}: failed at watch on {fileName}! message={es.Message}");
            }
        }

        public void Dispose()
        {
            foreach (var pair in _watchers)
            {
                pair.Value.Dispose();
            }
            _watchers.Clear();
            _logger.LogInformation($"{nameof(FileWatcher)}: {nameof(Dispose)}!");
        }
    }
}
