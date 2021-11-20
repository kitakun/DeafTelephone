namespace DeafTelephone.Web.Services.Services
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    
    using DeafTelephone.Web.Core.Services;
    using DeafTelephone.Web.Services.Persistence;

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

        public async Task ClearOldLogsAsync(CancellationToken token)
        {
            var currentDate = DateTime.UtcNow;
            var removeAllLogsOlderThan = currentDate.AddMonths(-1);
            var oldScopes = await _dbContext
                .LogScopes
                .Where(w => !w.RootScopeId.HasValue && w.CreatedAt < removeAllLogsOlderThan)
                .ToListAsync(token);

            _logger.LogInformation($"{nameof(LogCleanerService)}:Loaded {oldScopes.Count} scopes that will be deleted");

            if (oldScopes.Count > 0)
            {
                var oldScopeIds = oldScopes.Select(s => s.Id).ToList();

                const int chunkSize = 128;

                var allOldLogs = _dbContext
                    .Logs
                    .Where(w => w.RootScopeId.HasValue && oldScopeIds.Contains(w.RootScopeId.Value));

                var allLogsCount = await allOldLogs.CountAsync(token);
                _logger.LogInformation($"{nameof(LogCleanerService)}: Loaded {allLogsCount} logs that will be deleted");

                if (allLogsCount > 0)
                {
                    var allLogsChunk = (await allOldLogs.ToListAsync(token)).Chunk(chunkSize);
                    foreach (var chunkOfOldLogs in allLogsChunk)
                    {
                        _dbContext.RemoveRange(chunkOfOldLogs);
                        _logger.LogInformation($"{nameof(LogCleanerService)}:Removed allOldLogs {chunkOfOldLogs.Length}");

                        await _dbContext.SaveChangesAsync(token);
                    }
                }

                _dbContext.RemoveRange(oldScopes);
                _logger.LogInformation($"{nameof(LogCleanerService)}:Removed oldScopes {oldScopes.Count}");

                await _dbContext.SaveChangesAsync(token);

                _logger.LogInformation($"{nameof(LogCleanerService)}:Done with success");
            }
        }

        public async Task<string> GetDBSizeAsync(CancellationToken cancellationToken)
        {
            var dbConnection = _dbContext.Database.GetDbConnection();
            await using var command = dbConnection.CreateCommand();
            
            cancellationToken.ThrowIfCancellationRequested();

            command.CommandText = "select pg_size_pretty(pg_database_size('deaflogs'));";
            command.CommandType = CommandType.Text;

            await _dbContext.Database.OpenConnectionAsync(cancellationToken);

            await using var result = await command.ExecuteReaderAsync(cancellationToken);
            while (await result.ReadAsync(cancellationToken))
            {
                var response = result.GetString(0);
                return response;
            }

            return "?";
        }
    }
}