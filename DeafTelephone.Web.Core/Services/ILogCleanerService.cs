namespace DeafTelephone.Web.Core.Services
{
    using System.Threading.Tasks;

    /// <summary>
    /// Clear old data
    /// </summary>
    public interface ILogCleanerService
    {
        /// <summary>
        /// Clear old logs
        /// </summary>
        Task ClearOldLogs();
    }
}
