namespace DeafTelephone.Infrastructure.Logger.Serilog
{
    using global::Serilog;

    using Microsoft.Extensions.Hosting;

    public static class SerilogExtensions
    {
        public static ILogger CreateLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            return Log.Logger;
        }

        public static void Dispose()
        {
            Log.CloseAndFlush();
        }

        public static IHostBuilder ApplySerilogToTheProject(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseSerilog();
        }
    }
}
