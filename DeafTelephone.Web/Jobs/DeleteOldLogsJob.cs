namespace DeafTelephone.Web.Jobs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using DeafTelephone.Web.Core.Services;

    using Microsoft.Extensions.Logging;

    public class DeleteOldLogsJob : IScopedJob, IDisposable
    {
        private readonly ILogger<DeleteOldLogsJob> _logger;
        private readonly ILogCleanerService _cleanerService;

        private Timer _timer;

        public DeleteOldLogsJob(
            ILogger<DeleteOldLogsJob> logger,
            ILogCleanerService cleanerService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cleanerService = cleanerService ?? throw new ArgumentNullException(nameof(cleanerService));
        }

        public async Task Launch()
        {
            _logger.LogInformation($"Job {nameof(DeleteOldLogsJob)} step is started.");
            try
            {
                await _cleanerService.ClearOldLogs();
            }
            catch (Exception es)
            {
                _logger.LogCritical($"Job {nameof(DeleteOldLogsJob)} Exception in job! {es.Message}", es);
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
