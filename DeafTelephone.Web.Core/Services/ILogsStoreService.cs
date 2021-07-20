﻿namespace DeafTelephone.Web.Core.Services
{
    using System.Threading.Tasks;

    using DeafTelephone.Web.Core.Domain;

    public interface ILogsStoreService
    {
        Task<LogRecord> InsertAsync(LogRecord newRecord);
    }
}
