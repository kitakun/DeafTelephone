namespace DeafTelephone.Web.Jobs
{
    using DeafTelephone.Web.Core.Services;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Threading.Tasks;

    public class DeleteOldLogsJob : IScopedJob
    {
        private readonly ILogger<DeleteOldLogsJob> _logger;
        private readonly ILogCleanerService _cleanerService;

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
    }
}
