namespace DeafTelephone.Web.Core.Services
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Clear old data
    /// </summary>
    public interface ILogCleanerService
    {
        /// <summary>
        /// Get database size as string
        /// </summary>
        Task<string> GetDBSizeAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Clear old logs
        /// </summary>
        Task ClearOldLogsAsync(CancellationToken token);
    }
}
