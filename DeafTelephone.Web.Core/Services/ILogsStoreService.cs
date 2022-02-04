namespace DeafTelephone.Web.Core.Services
{
    using DeafTelephone.Web.Core.Domain;
    using DeafTelephone.Web.Core.Models;

    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ILogsStoreService
    {
        /// <summary>
        /// Save log record in to database
        /// </summary>
        Task<LogRecord> InsertLogRecordAsync(LogRecord newRecord);

        /// <summary>
        /// Create log scope in to database
        /// </summary>
        /// <param name="rootScopeId">root scope ID</param>
        /// <param name="ownerScopeId">scope owner ID</param>
        /// <param name="createdAt">dateTime when this scope was created (could be null)</param>
        ValueTask<LogScopeRecord> CreateScope(long? rootScopeId = null, long? ownerScopeId = null, DateTime? createdAt = null);

        /// <summary>
        /// Create root log scope inside <c>Project Scope</c>
        /// </summary>
        /// <param name="project">Project (could be application name)</param>
        /// <param name="environment">Environment (could be test/prod/etc)</param>
        /// <param name="createdAt">dateTime when this scope was created (could be null)</param>
        ValueTask<LogScopeRecord> CreateRootScope(string project, string environment, DateTime? createdAt = null);

        /// <summary>
        /// Load logs
        /// </summary>
        Task<(List<LogScopeRecord>, List<LogRecord>)> Fetch(LogFetchFilters filters, CancellationToken token);
    }
}
