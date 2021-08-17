namespace DeafTelephone.Web.Jobs
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using DeafTelephone.Web.Core.Services;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class ScopedJobsLauncher : IHostedService, IDisposable
    {
        private readonly ILogger<ScopedJobsLauncher> _logger;
        private readonly IServiceProvider _serviceProvider;

        private CancellationToken _cancellation;
        private Timer _timer;

        public ScopedJobsLauncher(
            ILogger<ScopedJobsLauncher> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellation = cancellationToken;

            _logger.LogInformation($"Job {nameof(ScopedJobsLauncher)} is running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Job {nameof(ScopedJobsLauncher)} is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _cancellation.ThrowIfCancellationRequested();

            using var scope = _serviceProvider.CreateScope();

            var scopedProcessingService = scope.ServiceProvider.GetRequiredService<IEnumerable<IScopedJob>>();
            foreach (var asyncService in scopedProcessingService)
            {
                asyncService.Launch().GetAwaiter().GetResult();
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
