namespace DeafTelephone.Web.Services.Persistence
{
    using System.Reflection;

    using Microsoft.EntityFrameworkCore;

    using DeafTelephone.Web.Core.Domain;

    internal class LogDbContext : DbContext
    {
        public DbSet<LogRecord> Logs { get; set; }
        public DbSet<LogScopeRecord> LogScopes { get; set; }

        public DbSet<SettingRecord> Settigns { get; set; }

        public LogDbContext()
        {

        }

        public LogDbContext(DbContextOptions<LogDbContext> options)
        {

        }

#if DEBUG
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql("User ID=migrator;Password=migrator;Host=localhost;Port=5432;Database=deaflogs;Pooling=true;");
#endif

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder
                .ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
