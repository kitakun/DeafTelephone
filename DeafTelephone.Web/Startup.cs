namespace DeafTelephone
{
    using System.Reflection;

    using MediatR;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using DeafTelephone.Hubs;
    using DeafTelephone.Web.Infrastracture;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy(DeafConstants.CorsPolicyName, builder =>
            {
                builder.AllowAnyHeader()
                       .AllowAnyMethod()
                       // enable signalR cors
                       .SetIsOriginAllowed((host) => true)
                       .AllowCredentials();
            }));

            services.AddSignalR();
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddGrpc();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // enable gRPC Cors
            app.UseCors(DeafConstants.CorsPolicyName);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<LogiService>();
                endpoints.MapHub<LogHub>(LogHub.HUB_URL, (hubSetts) =>
                {
                    hubSetts.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
                });

                endpoints.MapGet("/", async context =>
                {
                    await context
                        .Response
                        .WriteAsync("Communication with gRPC endpoints must be made through a gRPC client.");
                });
            });
        }
    }
}
