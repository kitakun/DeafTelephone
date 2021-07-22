namespace DeafTelephone.Web.Extensions
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    using DeafTelephone.Web.Core.Services;

    public static class ProgramExtensions
    {
        public static IHost MigrateDbOnStartup(this IHost buildedApp)
        {
            using (var scope = buildedApp.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var context = services.GetRequiredService<IProgramService>();
                context.ApplyMigration();
            }

            return buildedApp;
        }
    }
}
