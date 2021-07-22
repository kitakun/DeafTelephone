namespace DeafTelephone.Web.Services.Services
{
    using System;

    using Microsoft.Extensions.Logging;
    using Microsoft.EntityFrameworkCore;

    using DeafTelephone.Web.Core.Services;
    using DeafTelephone.Web.Services.Persistence;

    internal class ProgramService : IProgramService
    {
        private readonly LogDbContext _dbContext;
        private readonly ILogger<ProgramService> _logger;

        public ProgramService(
            LogDbContext dbContext,
            ILogger<ProgramService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void ApplyMigration()
        {
            try
            {
                _dbContext.Database.Migrate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while seeding DataBase.");
#if Debug
                Console.ReadKey();
#endif
            }
        }
    }
}
