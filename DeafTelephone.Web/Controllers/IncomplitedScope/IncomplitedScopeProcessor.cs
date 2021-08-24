namespace DeafTelephone.Web.Controllers.IncomplitedScope
{
    using DeafTelephone.Hubs;
    using DeafTelephone.Web.Controllers.BulkLogOperation;
    using DeafTelephone.Web.Core.Domain;
    using DeafTelephone.Web.Core.Extensions;
    using DeafTelephone.Web.Core.Services;
    using DeafTelephone.Web.Hub.Models;

    using MediatR;

    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using static DeafTelephone.Web.Controllers.BulkLogOperation.BulkLogOperationProcessor;

    public class IncomplitedScopeProcessor : IRequestHandler<IncomplitedScopeQuery>
    {
        internal static string CACHE_LOCAL_SCOPE_MAP => BulkLogOperationProcessor.CACHE_LOCAL_SCOPE_MAP;

        private const string IncomplitedMessage = "Incomplete request. Something bad happened at logger";

        private readonly IMemoryCache _cache;
        private readonly ILogsStoreService _logStoreService;
        private readonly IHubContext<LogHub> _hubAccess;
        private readonly ILogger<IncomplitedScopeProcessor> _logger;

        public IncomplitedScopeProcessor(
            IMemoryCache cache,
            ILogsStoreService logStoreService,
            IHubContext<LogHub> hub,
            ILogger<IncomplitedScopeProcessor> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logStoreService = logStoreService ?? throw new ArgumentNullException(nameof(logStoreService));
            _hubAccess = hub ?? throw new ArgumentNullException(nameof(hub));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(IncomplitedScopeQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!string.IsNullOrEmpty(request.CacheKey))
            {
                // we should have cache for this request
                var cacheMap = _cache.Get<MapCacheItem>(request.CacheKey);

                var newRcord = new LogRecord()
                {
                    CreatedAt = DateTime.Now,
                    LogLevel = LogLevelEnum.Error,
                    Message = IncomplitedMessage.Truncate(255),
                    StackTrace = string.Empty,
                    ErrorTitle = IncomplitedMessage.Truncate(255),
                    OwnerScopeId = cacheMap.RootScopeId,
                    RootScopeId = cacheMap.RootScopeId,
                };

                await _logStoreService.InsertAsync(newRcord);

                _cache.Remove(request.CacheKey);

                await _hubAccess.Clients.Group(LogHub.ALL_LOGS_GROUP).SendAsync(
                            NewLogInScopeEvent.BROADCAST_LOG_MESSAGE_NAME, new NewLogInScopeEvent(newRcord), cancellationToken);
            }
            else
            {
                _logger.LogWarning($"Trying to incomplite log for cacheKey={request.CacheKey}, but there is no map for it");
            }

            return Unit.Value;
        }
    }
}
