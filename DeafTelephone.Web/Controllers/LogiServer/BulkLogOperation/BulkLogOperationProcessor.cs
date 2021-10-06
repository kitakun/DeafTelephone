namespace DeafTelephone.Web.Controllers.LogiServer.BulkLogOperation
{
    using DeafTelephone.Hubs;
    using DeafTelephone.Web.Core.Domain;
    using DeafTelephone.Web.Core.Extensions;
    using DeafTelephone.Web.Core.Services;
    using DeafTelephone.Web.Hub.Models;

    using MediatR;

    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    public class BulkLogOperationProcessor : IRequestHandler<BulkLogOperationQuery, BulkLogOperationResult>
    {
        internal const string CACHE_LOCAL_SCOPE_MAP = "CACHE_LOCAL_SCOPE_MAP_{0}";

        private readonly IMemoryCache _cache;
        private readonly ILogsStoreService _logStoreService;
        private readonly IHubContext<LogHub> _hubAccess;
        private readonly ILogger<BulkLogOperationProcessor> _logger;

        public BulkLogOperationProcessor(
            IMemoryCache cache,
            ILogsStoreService logStoreService,
            IHubContext<LogHub> hub,
            ILogger<BulkLogOperationProcessor> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logStoreService = logStoreService ?? throw new ArgumentNullException(nameof(logStoreService));
            _hubAccess = hub ?? throw new ArgumentNullException(nameof(hub));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<BulkLogOperationResult> Handle(BulkLogOperationQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            MapCacheItem cacheMap;
            string buildedCacheKey;

            if (!string.IsNullOrEmpty(request.Request.CacheKey))
            {
                buildedCacheKey = request.Request.CacheKey;

                // we should have cache for this request
                if (!_cache.TryGetValue<MapCacheItem>(buildedCacheKey, out var gettedCacheMap))
                    throw new Exception($"Can't find cacheKey={buildedCacheKey} in memory cache");

                cacheMap = gettedCacheMap;
            }
            else
            {
                // create new cache map
                cacheMap = new MapCacheItem();
                buildedCacheKey = string.Format(CACHE_LOCAL_SCOPE_MAP, cacheMap.CacheKey);
                using (var cacheEntry = _cache.CreateEntry(buildedCacheKey))
                {
                    cacheEntry.SetValue(cacheMap);
                    cacheEntry
                        .SetSlidingExpiration(TimeSpan.FromMinutes(15))
                        .SetAbsoluteExpiration(TimeSpan.FromHours(2));
                    cacheEntry.RegisterPostEvictionCallback(OnCacheEntryChanges);
                }

                _logger.LogInformation($"[{DateTime.Now:dd.MM.yyyy HH:mm}] Start new BuldOperation Messages.Count={request.Request.Messages.Count} SetCache.Key={buildedCacheKey}");
            }

            var scopeMapVal = cacheMap.CalculatedIds;
            var needToFinalize = false;
            for (var i = 0; i < request.Request.Messages.Count; i++)
            {
                var messageToProceed = request.Request.Messages[i];
                switch (messageToProceed.OperationType)
                {
                    case Server.BulkOperationType.CreateInitialScope:
                        if (!cacheMap.ScopeIdsMap.IsEmpty)
                            throw new Exception($"Can't create initial scope because it already should be existing!");

                        var targetProject = request.Request.Parameters[nameof(LogScopeRecord.Project)];
                        var targetEnv = request.Request.Parameters[nameof(LogScopeRecord.Environment)];

                        _logger.LogInformation($"[{DateTime.Now:dd.MM.yyyy HH:mm}] {nameof(Server.BulkOperationType)} {nameof(Server.BulkOperationType.CreateInitialScope)} proj={targetProject} env={targetEnv} Cache.Key={buildedCacheKey}");

                        var initialScope = await _logStoreService.CreateRootScope(targetProject, targetEnv, messageToProceed.CreatedAt.ToDateTime());

                        if (!cacheMap.ScopeIdsMap.TryAdd(++scopeMapVal, initialScope.Id))
                            throw new Exception($"Failed at setting initial scope id for {buildedCacheKey}");

                        cacheMap.RootScopeId = initialScope.Id;

                        await _hubAccess
                            .Clients
                            .Group(LogHub.ALL_LOGS_GROUP)
                            .SendAsync(NewScopeEvent.BROADCAST_NEW_SCOPE_MESSAGE, new NewScopeEvent(initialScope), cancellationToken);

                        break;

                    case Server.BulkOperationType.CreateScope:
                        var innerScope = await _logStoreService.CreateScope(
                            cacheMap.ScopeIdsMap[messageToProceed.RootScopeId],
                            cacheMap.ScopeIdsMap[messageToProceed.ScopeOwnerId],
                            messageToProceed.CreatedAt.ToDateTime());

                        if (!cacheMap.ScopeIdsMap.TryAdd(++scopeMapVal, innerScope.Id))
                            throw new Exception($"Failed at setting scope id ({scopeMapVal}) for {buildedCacheKey}");

                        await _hubAccess
                            .Clients
                            .Group(LogHub.ALL_LOGS_GROUP)
                            .SendAsync(NewScopeEvent.BROADCAST_NEW_SCOPE_MESSAGE, new NewScopeEvent(innerScope), cancellationToken);

                        break;

                    case Server.BulkOperationType.LogMessage:
                    case Server.BulkOperationType.LogException:
                        var newRcord = new LogRecord()
                        {
                            CreatedAt = messageToProceed.CreatedAt.ToDateTime(),
                            LogLevel = (LogLevelEnum)(int)messageToProceed.Level,
                            Message = messageToProceed.LogMessage.Truncate(255),
                            StackTrace = messageToProceed.ExceptionStackTrace.Truncate(1024),
                            ErrorTitle = messageToProceed.ExceptionMessage.Truncate(255),
                            OwnerScopeId = cacheMap.ScopeIdsMap[messageToProceed.ScopeOwnerId],
                            RootScopeId = cacheMap.ScopeIdsMap[messageToProceed.RootScopeId],
                        };

                        await _logStoreService.InsertLogRecordAsync(newRcord);

                        await _hubAccess
                            .Clients
                            .Group(LogHub.ALL_LOGS_GROUP)
                            .SendAsync(NewLogInScopeEvent.BROADCAST_LOG_MESSAGE_NAME, new NewLogInScopeEvent(newRcord), cancellationToken);

                        break;

                    case Server.BulkOperationType.FinalRequest:
                        needToFinalize = true;
                        break;
                }
            }

            cacheMap.CalculatedIds = scopeMapVal;

            if (needToFinalize)
            {
                _cache.Remove(buildedCacheKey);
                _logger.LogInformation($"[{DateTime.Now:dd.MM.yyyy HH:mm}] {nameof(Server.BulkOperationType)} {nameof(Server.BulkOperationType.FinalRequest)} Cache.Key={buildedCacheKey}");
            }

            return new BulkLogOperationResult()
            {
                CacheKey = buildedCacheKey
            };
        }

        private void OnCacheEntryChanges(object key, object value, EvictionReason reason, object state)
        {
            if(reason == EvictionReason.Expired
                || reason == EvictionReason.Removed
                || reason == EvictionReason.TokenExpired)
            {
                _logger.LogInformation($"{nameof(Server.BulkOperationType)} {nameof(OnCacheEntryChanges)} key={key} reason={reason}");
            }
        }

        public class MapCacheItem
        {
            public readonly ConcurrentDictionary<long, long> ScopeIdsMap;
            public readonly string CacheKey;
            public int CalculatedIds { get; set; }
            public long RootScopeId;

            public MapCacheItem()
            {
                CalculatedIds = 0;
                CacheKey = Guid.NewGuid().ToString();
                ScopeIdsMap = new ConcurrentDictionary<long, long>();
            }
        }
    }
}
