namespace DeafTelephone
{
    using System.Reflection;

    using MediatR;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Configuration;

    using DeafTelephone.Web.Infrastracture;
    using DeafTelephone.Web.Hub;
    using DeafTelephone.Web.Services.Infrastructure;

    public class Startup
    {
        public IConfiguration Configuration { get; init; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(o => o.EnableDetailedErrors = true);

            services.AddCors(o => o.AddPolicy(DeafConstants.CorsPolicyName, builder =>
            {
                builder.AllowAnyHeader()
                       .AllowAnyMethod()
                       // enable signalR cors
                       .SetIsOriginAllowed((host) => true)
                       .AllowCredentials();
            }));

            DeafServiceRegistration.ConfigureServices(services, Configuration);

            DeafHub.ConfigureServices(services);

            services.AddMediatR(Assembly.GetExecutingAssembly());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // enable gRPC Cors
            app.UseCors(DeafConstants.CorsPolicyName);
            app.UseGrpcWeb();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<LogiService>().EnableGrpcWeb();
                
                DeafHub.UseEndpoints(endpoints);

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
