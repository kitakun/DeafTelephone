namespace DeafTelephone.Web.Core.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using DeafTelephone.Web.Core.Domain;

    public interface ILogsStoreService
    {
        Task<LogRecord> InsertAsync(LogRecord newRecord);

        ValueTask<LogScopeRecord> CreateScope(long? rootScopeId = null, long? ownerScopeId = null);

        Task<(List<LogScopeRecord>, List<LogRecord>)> Fetch();
    }
}
