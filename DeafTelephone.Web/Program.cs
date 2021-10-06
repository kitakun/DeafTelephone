namespace DeafTelephone
{
    using DeafTelephone.Infrastructure.Logger.Serilog;
    using DeafTelephone.Web.Extensions;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = SerilogExtensions.CreateLogger();

            try
            {
                logger.Information($"Launch application");
                CreateHostBuilder(args)
                    .Build()
                    .MigrateDbOnStartup()
                    .Run();
            }
            finally
            {
                logger.Information($"Shutdown application");
                SerilogExtensions.Dispose();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .ApplySerilogToTheProject()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options =>
                    {
                        var configs = options.ApplicationServices.GetRequiredService<IConfiguration>();

                        var applyCert = !string.IsNullOrEmpty(configs["Certificates:Default:Path"]);

                        // enable gRpc access
                        options.ListenAnyIP(
                            configs.GetValue<int>("DeafSetts:GrpcPort"), o =>
                            {
                                o.Protocols = HttpProtocols.Http1AndHttp2;
                                if (applyCert)
                                {
                                    var certName = configs.GetSection("Certificates:Default:Path").Value;
                                    var certPass = configs.GetSection("Certificates:Default:Password").Value;

                                    o.UseHttps(certName, certPass);
                                }
                            });

                        // enable signalR access
                        options.ListenAnyIP(
                            configs.GetValue<int>("DeafSetts:SignalrPort"), o =>
                            {
                                o.Protocols = HttpProtocols.Http1AndHttp2;
                                if (applyCert)
                                {
                                    var certName = configs.GetSection("Certificates:Default:Path").Value;
                                    var certPass = configs.GetSection("Certificates:Default:Password").Value;

                                    o.UseHttps(certName, certPass);
                                }
                            });
                    });

                    webBuilder.UseStartup<Startup>();
                });
    }
}
