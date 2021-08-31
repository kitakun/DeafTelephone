namespace DeafTelephone.Web.Services.Services
{
    using DeafTelephone.Web.Core.Domain;
    using DeafTelephone.Web.Core.Services;
    using DeafTelephone.Web.Services.Persistence;

    using LinqKit;

    using Microsoft.EntityFrameworkCore;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    internal class LogsStoreService : ILogsStoreService
    {
        private readonly LogDbContext _dbContext;
        private const int TAKE_EVERY_ROOT_SCOPES = 4;

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

        public async ValueTask<LogScopeRecord> CreateScope(long? rootScopeId = null, long? ownerScopeId = null)
        {
            var newScope = new LogScopeRecord()
            {
                CreatedAt = DateTime.Now,
                OwnerScopeId = ownerScopeId,
                RootScopeId = rootScopeId,
            };

            var addedEntity = await _dbContext.AddAsync(newScope);

            await _dbContext.SaveChangesAsync();

            return addedEntity.Entity;
        }

        public async ValueTask<LogScopeRecord> CreateRootScope(string project, string environment)
        {
            var newScope = new LogScopeRecord()
            {
                CreatedAt = DateTime.Now,
                OwnerScopeId = null,
                RootScopeId = null,
                Project = project,
                Environment = environment,
            };

            var addedEntity = await _dbContext.AddAsync(newScope);

            await _dbContext.SaveChangesAsync();

            return addedEntity.Entity;
        }

        public async Task<(List<LogScopeRecord>, List<LogRecord>)> Fetch(int from)
        {
            var scopes = await _dbContext
                .LogScopes
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Where(w => !w.RootScopeId.HasValue)
                .Skip(from)
                .Take(TAKE_EVERY_ROOT_SCOPES)
                .ToListAsync();

            var rootScopeIds = scopes
                .Select(s => s.Id)
                .ToList();

            var childScopes = await _dbContext
                .LogScopes
                .AsNoTracking()
                .Where(w => w.RootScopeId.HasValue && rootScopeIds.Contains(w.RootScopeId.Value))
                .ToListAsync();

            // add child scopes to list
            scopes.AddRange(childScopes);

            var logMessages = await _dbContext
                .Logs
                .AsNoTracking()
                .Where(w => w.RootScopeId.HasValue && rootScopeIds.Contains(w.RootScopeId.Value))
                .ToListAsync();

            return (scopes, logMessages);
        }

        public async Task<(List<LogScopeRecord>, List<LogRecord>)> Fetch(
            int skipScopes,
            Expression<Func<LogRecord, bool>> predicateLogQuery,
            Expression<Func<LogScopeRecord, bool>> predicateRootScopeQuery)
        {
            if (predicateRootScopeQuery != null)
            {
                return await FetchByFilteredScopesAndLogs(skipScopes, predicateLogQuery, predicateRootScopeQuery);
            }

            if (predicateLogQuery != null)
            {
                return await FetchByFilteredLogs(skipScopes, predicateLogQuery);
            }

            return await Fetch(skipScopes);
        }

        private async Task<(List<LogScopeRecord>, List<LogRecord>)> FetchByFilteredLogs(
            int skipScopes,
            Expression<Func<LogRecord, bool>> predicateLogQuery)
        {
            var filteredLogsByPredicate = await _dbContext
                .LogScopes
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skipScopes)
                .Take(TAKE_EVERY_ROOT_SCOPES)
                .Where(w => w.InnerLogsCollection.Any())
                .SelectMany(sm => sm.InnerLogsCollection)
                .Where(predicateLogQuery)
                .ToListAsync();

            // no results
            if (filteredLogsByPredicate.Count == 0)
            {
                return (new List<LogScopeRecord>(0), new List<LogRecord>(0));
            }

            var rootScopeIdsByQuery = filteredLogsByPredicate
                .Select(s => s.RootScopeId.Value)
                .Distinct()
                .ToList();

            var rootScopes = await _dbContext
                .LogScopes
                .AsNoTracking()
                .Where(w => rootScopeIdsByQuery.Contains(w.Id))
                .AsExpandableEFCore()
                .ToListAsync();

            var childScopes = await _dbContext
                .LogScopes
                .AsNoTracking()
                .Where(w => w.RootScopeId.HasValue && rootScopeIdsByQuery.Contains(w.RootScopeId.Value))
                .ToListAsync();

            // add child scopes to list
            rootScopes.AddRange(childScopes);

            var logMessages = await _dbContext
                .Logs
                .AsNoTracking()
                .Where(w => w.RootScopeId.HasValue && rootScopeIdsByQuery.Contains(w.RootScopeId.Value))
                .ToListAsync();

            return (rootScopes, logMessages);
        }

        private async Task<(List<LogScopeRecord>, List<LogRecord>)> FetchByFilteredScopesAndLogs(
            int skipSkopes,
            Expression<Func<LogRecord, bool>> predicateLogQuery,
            Expression<Func<LogScopeRecord, bool>> predicateRootScopeQuery)
        {
            var rootLogsIdsWithFilteredQuery = _dbContext
                .LogScopes
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Where(predicateRootScopeQuery)
                .Skip(skipSkopes)
                .Where(w => w.InnerLogsCollection.Any())
                .AsExpandableEFCore();

            var rootScopeIdsByQuery = await rootLogsIdsWithFilteredQuery
                .Select(s => s.Id)
                .Distinct()
                .ToListAsync();

            // no results
            if (rootScopeIdsByQuery.Count == 0)
            {
                return (new List<LogScopeRecord>(0), new List<LogRecord>(0));
            }

            var rootScopes = await _dbContext
                .LogScopes
                .AsNoTracking()
                .Where(w => rootScopeIdsByQuery.Contains(w.Id))
                .AsExpandableEFCore()
                .ToListAsync();

            var childScopes = await _dbContext
                .LogScopes
                .AsNoTracking()
                .Where(w => w.RootScopeId.HasValue && rootScopeIdsByQuery.Contains(w.RootScopeId.Value))
                .ToListAsync();

            // add child scopes to list
            rootScopes.AddRange(childScopes);

            var logMessages = await _dbContext
                .Logs
                .AsNoTracking()
                .Where(w => w.RootScopeId.HasValue && rootScopeIdsByQuery.Contains(w.RootScopeId.Value))
                .Where(predicateLogQuery)
                .ToListAsync();

            return (rootScopes, logMessages);
        }
    }
}
