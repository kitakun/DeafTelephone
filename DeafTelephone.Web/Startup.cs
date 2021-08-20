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
    using DeafTelephone.Web.Services;
    using DeafTelephone.Web.Jobs;
    using Microsoft.Extensions.Logging;

    public class Startup
    {
        public IConfiguration Configuration { get; init; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(o =>
            {
                o.Interceptors.Add<WhitelistInterceptor>();
                o.EnableDetailedErrors = true;
            });

            services.AddCors(o => o.AddPolicy(DeafConstants.CorsPolicyName, builder =>
            {
                builder.AllowAnyHeader()
                       .AllowAnyMethod()
                       .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding")
                       // enable signalR cors
                       .SetIsOriginAllowed((host) => true)
                       .AllowCredentials();
            }));

            DeafServiceRegistration.ConfigureServices(services, Configuration);

            services.AddScoped<IScopedJob, DeleteOldLogsJob>();

            DeafHub.ConfigureServices(services);

            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.AddHostedService<ScopedJobsLauncher>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // enable gRPC Cors
            app.UseCors(DeafConstants.CorsPolicyName);
            app.UseGrpcWeb();

            logger.LogInformation($"Launch app with version {Assembly.GetExecutingAssembly().GetName().Version}");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<LogiService>().EnableGrpcWeb();
                endpoints.MapGrpcService<LogiClientService>().EnableGrpcWeb();

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
