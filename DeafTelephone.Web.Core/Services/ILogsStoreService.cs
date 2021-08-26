namespace DeafTelephone.Web.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
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
        /// Create root log scope inside <c>Project Scope</c>
        /// </summary>
        /// <param name="project">Project (could be application name)</param>
        /// <param name="environment">Environment (could be test/prod/etc)</param>
        ValueTask<LogScopeRecord> CreateRootScope(string project, string environment);

        /// <summary>
        /// Load logs
        /// </summary>
        /// <param name="from"></param>
        Task<(List<LogScopeRecord>, List<LogRecord>)> Fetch(
            int from,
            Expression<Func<LogRecord, bool>> predicateLogQuery,
            Expression<Func<LogScopeRecord, bool>> predicateRootScopeQuery);
    }
}
