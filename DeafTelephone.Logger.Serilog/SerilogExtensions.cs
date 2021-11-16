namespace DeafTelephone.Infrastructure.Logger.Serilog
{
    using global::Serilog;

    using Microsoft.Extensions.Hosting;

    public static class SerilogExtensions
    {
        /// <summary>
        /// Globally create a logger and return the instance
        /// </summary>
        public static ILogger CreateLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
#if RELEASE
                .WriteTo.File("log-.txt", rollingInterval: RollingInterval.Day)
#endif
                .CreateLogger();

            return Log.Logger;
        }

        /// <summary>
        /// Close logger
        /// </summary>
        public static void Dispose()
        {
            Log.CloseAndFlush();
        }

        /// <summary>
        /// Inject the logger into ASP.NET environment
        /// </summary>
        public static IHostBuilder ApplySerilogToTheProject(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseSerilog();
        }
    }
}
