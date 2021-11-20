namespace DeafTelephone.Web.Jobs
{
    using System;
    using System.Threading.Tasks;
    using System.Threading;
    
    using DeafTelephone.Web.Core.Services;

    using Microsoft.Extensions.Logging;

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

        public async Task LaunchAsync(CancellationToken token)
        {
            _logger.LogInformation($"Job {nameof(DeleteOldLogsJob)} step is started.");
            try
            {
                _logger.LogInformation($"DB Size: {await _cleanerService.GetDBSizeAsync(token)}.");

                await _cleanerService.ClearOldLogsAsync(token);
            }
            catch (Exception es)
            {
                _logger.LogCritical($"Job {nameof(DeleteOldLogsJob)} Exception in job! {es.Message}", es);
            }
        }
    }
}
