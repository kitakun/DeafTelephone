namespace DeafTelephone.Web.Services.Services
{
    using DeafTelephone.Web.Core.Domain;
    using DeafTelephone.Web.Core.Models;
    using DeafTelephone.Web.Core.Services;
    using DeafTelephone.Web.Services.Persistence;

    using LinqKit;

    using Microsoft.EntityFrameworkCore;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal class LogsStoreService : ILogsStoreService
    {
        private readonly LogDbContext _dbContext;
        private const int TAKE_EVERY_ROOT_SCOPES = 20;

        public LogsStoreService(LogDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<LogRecord> InsertLogRecordAsync(LogRecord newRecord)
        {
            var addedEntity = await _dbContext.AddAsync(newRecord);

            await _dbContext.SaveChangesAsync();

            return addedEntity.Entity;
        }

        public async ValueTask<LogScopeRecord> CreateScope(long? rootScopeId = null, long? ownerScopeId = null, DateTime? createdAt = null)
        {
            var newScope = new LogScopeRecord()
            {
                CreatedAt = createdAt ?? DateTime.Now,
                OwnerScopeId = ownerScopeId,
                RootScopeId = rootScopeId,
            };

            var addedEntity = await _dbContext.AddAsync(newScope);

            await _dbContext.SaveChangesAsync();

            return addedEntity.Entity;
        }

        public async ValueTask<LogScopeRecord> CreateRootScope(string project, string environment, DateTime? createdAt = null)
        {
            var newScope = new LogScopeRecord()
            {
                CreatedAt = createdAt ?? DateTime.Now,
                OwnerScopeId = null,
                RootScopeId = null,
                Project = project,
                Environment = environment,
            };

            var addedEntity = await _dbContext.AddAsync(newScope);

            await _dbContext.SaveChangesAsync();

            return addedEntity.Entity;
        }

        public async Task<(List<LogScopeRecord>, List<LogRecord>)> Fetch(LogFetchFilters filters, CancellationToken token)
        {
            if (filters.PredicateRootScopeQuery != null || filters.PredicateLogQuery != null)
            {
                return await FilteredFetch(filters, token);
            }

            return await Fetch(
                filters.SkipScopes ?? 0,
                filters.TakeScopes ?? TAKE_EVERY_ROOT_SCOPES,
                token);
        }

        private async Task<(List<LogScopeRecord>, List<LogRecord>)> Fetch(int from, int take, CancellationToken token)
        {
            var scopes = await _dbContext
                .LogScopes
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Where(w => !w.RootScopeId.HasValue)
                .Skip(from)
                .Take(take)
                .ToListAsync(token);

            var rootScopeIds = scopes
                .Select(s => s.Id)
                .ToList();

            var childScopes = await _dbContext
                .LogScopes
                .AsNoTracking()
                .Where(w => w.RootScopeId.HasValue && rootScopeIds.Contains(w.RootScopeId.Value))
                .ToListAsync(token);

            // add child scopes to list
            scopes.AddRange(childScopes);

            var logMessages = await _dbContext
                .Logs
                .AsNoTracking()
                .Where(w => w.RootScopeId.HasValue && rootScopeIds.Contains(w.RootScopeId.Value))
                .ToListAsync(token);

            return (scopes, logMessages);
        }

        private async Task<(List<LogScopeRecord>, List<LogRecord>)> FilteredFetch(LogFetchFilters filters, CancellationToken token)
        {
            // build query for log fetching
            var filteredLogMessagesQuery = _dbContext
                .Logs
                .AsNoTracking()
                .Where(w => w.RootScopeId.HasValue);

            if(filters.PredicateLogQuery != null)
            {
                filteredLogMessagesQuery = filteredLogMessagesQuery.Where(filters.PredicateLogQuery);
            }

            var filteredLogMessages = filteredLogMessagesQuery.Select(s => s.RootScopeId.Value);

            // build query for root scope fetching

            var rootScopeIdsWithFilteredQuery = _dbContext
                .LogScopes
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .AsExpandableEFCore();
            if(filters.PredicateRootScopeQuery != null)
            {
                rootScopeIdsWithFilteredQuery = rootScopeIdsWithFilteredQuery.Where(filters.PredicateRootScopeQuery);
            }

            rootScopeIdsWithFilteredQuery = rootScopeIdsWithFilteredQuery
                .Skip(filters.SkipScopes ?? 0)
                .Where(w => filteredLogMessages.Contains(w.Id))
                .Take(filters.TakeScopes ?? TAKE_EVERY_ROOT_SCOPES);

            var rootScopes = await rootScopeIdsWithFilteredQuery
                .ToListAsync(token);

            // no results
            if (rootScopes.Count == 0)
            {
                return (new List<LogScopeRecord>(0), new List<LogRecord>(0));
            }

            var rootScopeIds = rootScopes.Select(s => s.Id).ToList();

            // load child scopes
            var childScopes = await _dbContext
                .LogScopes
                .AsNoTracking()
                .Where(w => w.RootScopeId.HasValue && rootScopeIds.Contains(w.RootScopeId.Value))
                .ToListAsync(token);

            rootScopes.AddRange(childScopes);

            // load all logs
            var logMessages = await _dbContext
                .Logs
                .AsNoTracking()
                .Where(w => w.RootScopeId.HasValue && rootScopeIds.Contains(w.RootScopeId.Value))
                .ToListAsync(token);

            return (rootScopes, logMessages);
        }
    }
}
