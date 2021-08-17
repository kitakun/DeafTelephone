namespace DeafTelephone.Web.Core.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using DeafTelephone.Web.Core.Domain;

    public interface ILogsStoreService
    {
        /// <summary>
        /// Save log record in to database
        /// </summary>
        Task<LogRecord> InsertAsync(LogRecord newRecord);

        /// <summary>
        /// Create log scope in to database
        /// </summary>
        /// <param name="rootScopeId">root scope ID</param>
        /// <param name="ownerScopeId">scope owner ID</param>
        ValueTask<LogScopeRecord> CreateScope(long? rootScopeId = null, long? ownerScopeId = null);

        /// <summary>
        /// Load logs
        /// </summary>
        /// <param name="from"></param>
        Task<(List<LogScopeRecord>, List<LogRecord>)> Fetch(int from);
        /// <summary>
        /// Load logs with filters
        /// </summary>
        /// <param name="from">skip logs</param>
        /// <param name="query">search for string</param>
        Task<(List<LogScopeRecord>, List<LogRecord>)> Fetch(int from, string query);
    }
}
