namespace DeafTelephone.Hubs
{
    using DeafTelephone.Web.Core.Services.Security;
    using DeafTelephone.Web.Hub.Hubs;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;

    using System.Threading.Tasks;

    [AllowAnonymous]
    public class LogHub : Hub<ILogHub>
    {
        internal const string HUB_URL = "/logHub";
        public const string ALL_LOGS_GROUP = "all_logs";

        private readonly IWhitelistService _whitelistService;

        public LogHub(IWhitelistService whitelistService)
        {
            _whitelistService = whitelistService ?? throw new System.ArgumentNullException(nameof(whitelistService));
        }

        public override async Task OnConnectedAsync()
        {
            if (await IsHttpClientAllowedToListenAsync())
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, ALL_LOGS_GROUP);
            }
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Is client allowed to listen SignalR messages
        /// </summary>
        /// <returns>true or false</returns>
        public ValueTask<bool> IsAllowed()
        {
            return IsHttpClientAllowedToListenAsync();
        }

        private ValueTask<bool> IsHttpClientAllowedToListenAsync()
        {
            // validate access by WhiteList API list
            var httpContext = Context.GetHttpContext();
            var clientIP = httpContext.Connection.RemoteIpAddress;
            var address = clientIP.ToString();

            const string ipv6Submask = "::ffff:";
            if (address.Length > 6 && address.Substring(0, ipv6Submask.Length) == ipv6Submask)
            {
                address = address[ipv6Submask.Length..];
            }

            return _whitelistService.IsAllowedAsync(address);
        }
    }
}
