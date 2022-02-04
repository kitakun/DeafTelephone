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
    using Persistence;
    using Core.Extensions;

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
            var removeAllLogsOlderThan = DateTime.UtcNow.AddMonths(-1);

            await DeleteOldLogsWithScopesAsync(removeAllLogsOlderThan, token);
            
            if (token.IsCancellationRequested)
                return;

            await DeleteEmptyScopesAsync(removeAllLogsOlderThan, token);
        }

        private async ValueTask DeleteOldLogsWithScopesAsync(DateTime removeAllLogsOlderThan, CancellationToken token)
        {
            var oldScopes = await _dbContext
                .LogScopes
                .Where(w => !w.RootScopeId.HasValue && w.CreatedAt < removeAllLogsOlderThan)
                .ToListAsync(token);

            _logger.LogInformation($"{nameof(LogCleanerService)} m:{nameof(DeleteOldLogsWithScopesAsync)}:Loaded {oldScopes.Count} scopes that will be deleted");

            if (oldScopes.Count > 0)
            {
                var oldScopeIds = oldScopes.Select(s => s.Id).ToList();

                const int chunkSize = 128;

                var allOldLogs = _dbContext
                    .Logs
                    .Where(w => w.RootScopeId.HasValue && oldScopeIds.Contains(w.RootScopeId.Value));

                var allLogsCount = await allOldLogs.CountAsync(token);
                _logger.LogInformation($"{nameof(DeleteOldLogsWithScopesAsync)}: Loaded {allLogsCount} logs that will be deleted");

                if (allLogsCount > 0)
                {
                    var allLogsChunk = (await allOldLogs.ToListAsync(token)).ChunkBy(chunkSize);
                    foreach (var chunkOfOldLogs in allLogsChunk)
                    {
                        _dbContext.RemoveRange(chunkOfOldLogs);
                        _logger.LogInformation($"{nameof(DeleteOldLogsWithScopesAsync)}:Removed allOldLogs {chunkOfOldLogs.Count}");

                        await _dbContext.SaveChangesAsync(token);
                    }
                }

                _dbContext.RemoveRange(oldScopes);
                _logger.LogInformation($"{nameof(DeleteOldLogsWithScopesAsync)}:Removed oldScopes {oldScopes.Count}");

                await _dbContext.SaveChangesAsync(token);

                _logger.LogInformation($"{nameof(DeleteOldLogsWithScopesAsync)}:Done with success");
            }
        }

        private async ValueTask DeleteEmptyScopesAsync(DateTime removeAllLogsOlderThan, CancellationToken token)
        {
            var oldEmptyScopes = await _dbContext
                .LogScopes
                .Where(w => w.RootScopeId.HasValue
                        && w.CreatedAt < removeAllLogsOlderThan
                        && w.InnerLogsCollection.Count == 0)
                .ToListAsync(token);

            _logger.LogInformation($"{nameof(LogCleanerService)} m:{nameof(DeleteEmptyScopesAsync)}:Loaded {oldEmptyScopes.Count} empty scopes that will be deleted");

            if(oldEmptyScopes.Count > 0)
            {
                _dbContext.RemoveRange(oldEmptyScopes);
                _logger.LogInformation($"{nameof(DeleteEmptyScopesAsync)}:Removed old empty Scopes {oldEmptyScopes.Count}");

                await _dbContext.SaveChangesAsync(token);

                _logger.LogInformation($"{nameof(DeleteEmptyScopesAsync)}:Done with success");
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