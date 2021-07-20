namespace DeafTelephone.Web.Services.Persistence
{
    using DeafTelephone.Web.Core.Domain;

    using Microsoft.EntityFrameworkCore;

    internal class LogDbContext : DbContext
    {
        public DbSet<LogRecord> Logs { get; set; }
        public DbSet<LogScopeRecord> LogScopes { get; set; }

#if DEBUG
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql("Host=my_host2;Database=my_db;Username=my_user;Password=my_pw");
#endif
    }
}
