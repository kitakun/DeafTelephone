namespace DeafTelephone.Web.Services.Infrastructure
{
    using DeafTelephone.Web.Core.Services;
    using DeafTelephone.Web.Services.Persistence;
    using DeafTelephone.Web.Services.Services;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class DeafServiceRegistration
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IMemoryCache, MemoryCache>();
            services.AddScoped<ILogsStoreService, LogsStoreService>();
            services.AddScoped<IProgramService, ProgramService>();

            services.AddDbContext<LogDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString(nameof(LogDbContext))));
        }
    }
}
