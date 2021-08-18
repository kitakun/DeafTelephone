namespace DeafTelephone.Web.Services.Services
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;

    using DeafTelephone.Web.Core.Services;
    using DeafTelephone.Web.Services.Persistence;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    internal class LogCleanerService : ILogCleanerService
    {
        private readonly LogDbContext _dbContext;
        private readonly ILogger<LogCleanerService> _logger;

        public LogCleanerService(
            LogDbContext dbContext,
            ILogger<LogCleanerService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ClearOldLogs()
        {
            var currentDate = DateTime.UtcNow;
            var removeAllLogsOlderThan = currentDate.AddMonths(-1);
            var oldScopes = await _dbContext
                .LogScopes
                .Where(w => !w.RootScopeId.HasValue && w.CreatedAt < removeAllLogsOlderThan)
                .ToListAsync();

            _logger.LogInformation($"Loaded {oldScopes.Count} scopes that will be deleted");

            if (oldScopes.Count > 0)
            {
                var oldScopeIds = oldScopes.Select(s => s.Id).ToList();

                var allOldLogs = await _dbContext
                    .Logs
                    .Where(w => w.RootScopeId.HasValue && oldScopeIds.Contains(w.RootScopeId.Value))
                    .ToListAsync();

                _logger.LogInformation($"Loaded {allOldLogs.Count} logs that will be deleted");

                _dbContext.RemoveRange(allOldLogs);
                _dbContext.RemoveRange(oldScopes);

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Done with success");
            }
        }

        public async Task<string> GetDBSize()
        {
            var dbConnection = _dbContext.Database.GetDbConnection();
            using var command = dbConnection.CreateCommand();
            {
                command.CommandText = "select pg_size_pretty(pg_database_size('deaflogs'));";
                command.CommandType = CommandType.Text;

                await _dbContext.Database.OpenConnectionAsync();

                using (var result = await command.ExecuteReaderAsync())
                {
                    while (result.Read())
                    {
                        var response = result.GetString(0);
                        return response;
                    }
                }
            }
            return "?";
        }
    }
}
