namespace DeafTelephone.Web.Hub
{
    using DeafTelephone.Hubs;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http.Connections;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;

    public static class DeafHub
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
        }

        public static void UseEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHub<LogHub>(LogHub.HUB_URL, (hubSetts) =>
            {
                hubSetts.Transports = HttpTransportType.LongPolling;
            });
        }
    }
}
