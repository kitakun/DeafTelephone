namespace DeafTelephone.Web.Services.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using DeafTelephone.Web.Core.Domain;
    using DeafTelephone.Web.Core.Services;
    using DeafTelephone.Web.Services.Persistence;

    using Microsoft.EntityFrameworkCore;

    internal class LogsStoreService : ILogsStoreService
    {
        private readonly LogDbContext _dbContext;

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

        public async Task<(List<LogScopeRecord>, List<LogRecord>)> Fetch()
        {
            var scopes = await _dbContext.LogScopes.Take(50).ToListAsync();
            return (scopes, await _dbContext.Logs.Take(50).ToListAsync());
        }

        public async Task<LogRecord> InsertAsync(LogRecord newRecord)
        {
            var addedEntity = await _dbContext.AddAsync(newRecord);

            await _dbContext.SaveChangesAsync();

            return addedEntity.Entity;
        }
    }
}
