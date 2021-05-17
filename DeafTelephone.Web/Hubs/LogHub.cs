namespace DeafTelephone.Hubs
{
    using System.Threading.Tasks;

    using DeafTelephone.Models;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;

    [AllowAnonymous]
    public class LogHub : Hub
    {
        internal const string HUB_URL = "/logHub";

        private const string CLIENT_MESSAGE_RECIEVE_NAME = "OnReceiveLog";

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public async Task SendMessage(LogModel log)
        {
            await Clients.All.SendAsync(CLIENT_MESSAGE_RECIEVE_NAME, log);
        }

        public Task Hi(string msg)
        {
            return Task.CompletedTask;
        }
    }
}
