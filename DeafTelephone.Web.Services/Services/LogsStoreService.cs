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
        private const int TAKE_EVERY_ROOT_SCOPES = 20;

        public LogsStoreService(LogDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
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

        public async Task<LogRecord> InsertAsync(LogRecord newRecord)
        {
            var addedEntity = await _dbContext.AddAsync(newRecord);

            await _dbContext.SaveChangesAsync();

            return addedEntity.Entity;
        }

        public async Task<(List<LogScopeRecord>, List<LogRecord>)> Fetch(
            int from,
            Expression<Func<LogRecord, bool>> predicateLogQuery,
            Expression<Func<LogScopeRecord, bool>> predicateRootScopeQuery)
        {
            var initialLogQuery = _dbContext
                .Logs
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Where(w => w.RootScopeId.HasValue)
                .AsExpandableEFCore();

            if (predicateLogQuery != null)
            {
                initialLogQuery = initialLogQuery.Where(predicateLogQuery);
            }

            var logsByQueryMessages = await initialLogQuery.ToListAsync();

            var rootScopeIdsByQuery = logsByQueryMessages
                .Where(s => s.RootScopeId.HasValue)
                .Select(s => s.RootScopeId.Value)
                .Distinct()
                .ToList();

            var initialRootScopeQuery = _dbContext
                .LogScopes
                .AsNoTracking()
                .Where(w => rootScopeIdsByQuery.Contains(w.Id))
                .AsExpandableEFCore();

            if (initialRootScopeQuery != null)
            {
                initialRootScopeQuery = initialRootScopeQuery.Where(predicateRootScopeQuery);
            }

            var scopes = await initialRootScopeQuery
                .Skip(from)
                .Take(TAKE_EVERY_ROOT_SCOPES)
                .ToListAsync();

            // no results
            if (scopes.Count == 0)
            {
                return (new List<LogScopeRecord>(0), new List<LogRecord>(0));
            }

            if (predicateLogQuery != null || predicateRootScopeQuery != null)
            {
                // update root scope Id because it might be changed after predicates
                rootScopeIdsByQuery = scopes.Select(s => s.Id).Distinct().ToList();
            }

            var childScopes = await _dbContext
                .LogScopes
                .AsNoTracking()
                .Where(w => w.RootScopeId.HasValue && rootScopeIdsByQuery.Contains(w.RootScopeId.Value))
                .ToListAsync();

            // add child scopes to list
            scopes.AddRange(childScopes);

            var logMessages = await _dbContext
                .Logs
                .AsNoTracking()
                .Where(w => w.RootScopeId.HasValue && rootScopeIdsByQuery.Contains(w.RootScopeId.Value))
                .ToListAsync();

            return (scopes, logMessages);
        }
    }
}
