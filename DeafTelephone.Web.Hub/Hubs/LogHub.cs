namespace DeafTelephone.Hubs
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;

    [AllowAnonymous]
    public class LogHub : Hub
    {
        internal const string HUB_URL = "/logHub";

        //public Task Hi(string msg)
        //{
        //    return Task.CompletedTask;
        //}
    }
}
