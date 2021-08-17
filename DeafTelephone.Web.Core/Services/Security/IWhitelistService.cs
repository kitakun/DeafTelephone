namespace DeafTelephone.Web.Core.Services.Security
{
    using System.Threading.Tasks;

    public interface IWhitelistService
    {
        ValueTask<bool> IsAllowedAsync(string address);
    }
}
