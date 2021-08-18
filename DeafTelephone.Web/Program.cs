namespace DeafTelephone
{
    using DeafTelephone.Web.Extensions;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class Program
    {
        public static void Main(string[] args) =>
            CreateHostBuilder(args)
                .Build()
                .MigrateDbOnStartup()
                .Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options =>
                    {
                        var configs = options.ApplicationServices.GetRequiredService<IConfiguration>();

                        // enable gRpc access
                        options.ListenAnyIP(configs.GetValue<int>("DeafSetts:GrpcPort"), o => o.Protocols = HttpProtocols.Http1AndHttp2);

                        // enable signalR access
                        options.ListenAnyIP(configs.GetValue<int>("DeafSetts:SignalrPort"), o => o.Protocols = HttpProtocols.Http1AndHttp2);
                    });

                    webBuilder.UseStartup<Startup>();
                });
    }
}
