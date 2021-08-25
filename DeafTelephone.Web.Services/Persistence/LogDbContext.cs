namespace DeafTelephone.Web.Services.Persistence
{
    using System.Reflection;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    using DeafTelephone.Web.Core.Domain;

    public sealed class LogDbContext : DbContext
    {
        private readonly IConfiguration _appConfigs;

        public DbSet<LogRecord> Logs { get; set; }
        public DbSet<LogScopeRecord> LogScopes { get; set; }

        public DbSet<SettingRecord> Settigns { get; set; }

        public LogDbContext()
        {

        }

        public LogDbContext(
            DbContextOptions<LogDbContext> options,
            IConfiguration appConfigs)
        {
            _appConfigs = appConfigs;
        }

#if DEBUG
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql("User ID=migrator;Password=migrator;Host=localhost;Port=5432;Database=deaflogs;Pooling=true;");
#elif RELEASE
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(_appConfigs.GetConnectionString("LogDbContext"));
#endif

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder
                .ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
