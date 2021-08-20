namespace DeafTelephone.Web.Controllers.BulkLogOperation
{
    using DeafTelephone.Hubs;
    using DeafTelephone.Web.Core.Domain;
    using DeafTelephone.Web.Core.Extensions;
    using DeafTelephone.Web.Core.Services;
    using DeafTelephone.Web.Hub.Models;

    using MediatR;

    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Caching.Memory;

    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class BulkLogOperationProcessor : IRequestHandler<BulkLogOperationQuery, BulkLogOperationResult>
    {
        internal const string CACHE_LOCAL_SCOPE_MAP = "CACHE_LOCAL_SCOPE_MAP_{0}";

        private readonly IMemoryCache _cache;
        private readonly ILogsStoreService _logStoreService;
        private readonly IHubContext<LogHub> _hubAccess;

        public BulkLogOperationProcessor(
            IMemoryCache cache,
            ILogsStoreService logStoreService,
            IHubContext<LogHub> hub)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logStoreService = logStoreService ?? throw new ArgumentNullException(nameof(logStoreService));
            _hubAccess = hub ?? throw new ArgumentNullException(nameof(hub));
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
                {
                    throw new Exception($"Can't find cacheKey={buildedCacheKey} in memory cache");
                }
                cacheMap = gettedCacheMap;
            }
            else
            {
                // create new cache map
                cacheMap = new MapCacheItem();
                buildedCacheKey = string.Format(CACHE_LOCAL_SCOPE_MAP, cacheMap.CacheKey);
                _cache.Set(buildedCacheKey, cacheMap);
            }

            var scopeMapVal = cacheMap.CalculatedIds;
            for (var i = 0; i < request.Request.Messages.Count; i++)
            {
                var messageToProceed = request.Request.Messages[i];
                switch (messageToProceed.OperationType)
                {
                    case Server.BulkOperationType.CreateInitialScope:
                        if (cacheMap.ScopeIdsMap.Count != 0)
                            throw new Exception($"Can't create initial scope because it already should be existing!");

                        var initialScope = await _logStoreService.CreateRootScope(
                            request.Request.Parameters[nameof(LogScopeRecord.Project)],
                            request.Request.Parameters[nameof(LogScopeRecord.Environment)]);

                        cacheMap.ScopeIdsMap.Add(++scopeMapVal, initialScope.Id);
                        cacheMap.RootScopeId = initialScope.Id;

                        await _hubAccess.Clients.All.SendAsync(
                            NewScopeEvent.BROADCAST_NEW_SCOPE_MESSAGE, new NewScopeEvent(initialScope), cancellationToken);

                        break;

                    case Server.BulkOperationType.CreateScope:
                        var innerScope = await _logStoreService.CreateScope(
                            cacheMap.ScopeIdsMap[messageToProceed.RootScopeId],
                            cacheMap.ScopeIdsMap[messageToProceed.ScopeOwnerId]);

                        cacheMap.ScopeIdsMap.Add(++scopeMapVal, innerScope.Id);

                        await _hubAccess.Clients.All.SendAsync(
                            NewScopeEvent.BROADCAST_NEW_SCOPE_MESSAGE, new NewScopeEvent(innerScope), cancellationToken);
                        break;

                    case Server.BulkOperationType.LogMessage:
                    case Server.BulkOperationType.LogException:
                        var newRcord = new LogRecord()
                        {
                            CreatedAt = DateTime.Now,
                            LogLevel = (LogLevelEnum)(int)messageToProceed.Level,
                            Message = messageToProceed.LogMessage.Truncate(255),
                            StackTrace = messageToProceed.ExceptionStackTrace.Truncate(1024),
                            ErrorTitle = messageToProceed.ExceptionMessage.Truncate(255),
                            OwnerScopeId = cacheMap.ScopeIdsMap[messageToProceed.ScopeOwnerId],
                            RootScopeId = cacheMap.ScopeIdsMap[messageToProceed.RootScopeId],
                        };

                        await _logStoreService.InsertAsync(newRcord);

                        await _hubAccess.Clients.All.SendAsync(
                            NewLogInScopeEvent.BROADCAST_LOG_MESSAGE_NAME, new NewLogInScopeEvent(newRcord), cancellationToken);

                        break;

                    case Server.BulkOperationType.FinalRequest:
                        _cache.Remove(buildedCacheKey);
                        break;
                }
            }

            cacheMap.CalculatedIds = scopeMapVal;

            return new BulkLogOperationResult()
            {
                CacheKey = buildedCacheKey
            };
        }

        public class MapCacheItem
        {
            public readonly Dictionary<long, long> ScopeIdsMap;
            public readonly string CacheKey;
            public int CalculatedIds { get; set; }
            public long RootScopeId;

            public MapCacheItem()
            {
                CalculatedIds = 0;
                CacheKey = Guid.NewGuid().ToString();
                ScopeIdsMap = new Dictionary<long, long>(4);
            }
        }
    }
}
