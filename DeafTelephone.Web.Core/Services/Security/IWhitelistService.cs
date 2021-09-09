namespace DeafTelephone.Web.Core.Services.Security
{
    using System.Threading.Tasks;

    public interface IWhitelistService
    {
        /// <summary>
        /// Is idAddress allowed to have access
        /// </summary>
        /// <param name="address">IP address</param>
        /// <returns>has access or not</returns>
        ValueTask<bool> IsAllowedAsync(string address);

        /// <summary>
        /// Clear global cache if it exists
        /// </summary>
        void ClearCache();
    }
}
