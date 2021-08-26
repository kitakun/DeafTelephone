namespace DeafTelephone.Web.Controllers.LogiClient.Hello
{
    using DeafTelephone.ForClient;
    using DeafTelephone.Web.Core.Services;
    using DeafTelephone.Web.Services.Persistence;

    using MediatR;

    using Microsoft.EntityFrameworkCore;

    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class HelloProcessor : IRequestHandler<HelloQuery, HelloResponse>
    {
        private readonly LogDbContext _dbContext;
        private readonly ILogCleanerService _logCleanService;

        public HelloProcessor(
            LogDbContext dbContext,
            ILogCleanerService logCleanService)
        {
            _dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext));
            _logCleanService = logCleanService ?? throw new System.ArgumentNullException(nameof(logCleanService));
        }

        public async Task<HelloResponse> Handle(HelloQuery request, CancellationToken cancellationToken)
        {
            // get all envs + projects
            var allEnvsAndProjects = await _dbContext.LogScopes
                .AsNoTracking()
                .Where(w => !w.RootScopeId.HasValue && w.Environment != null && w.Project != null)
                .Select(s => new { s.Environment, s.Project })
                .Distinct()
                .ToListAsync(cancellationToken);

            // group by env + all projects in env
            var map = allEnvsAndProjects
                .GroupBy(g => g.Environment)
                    .ToDictionary(
                        k => k.Key,
                        v => v.Select(s2 => s2.Project).ToList());

            // create response model

            var result = new HelloResponse()
            {
                DatabaseSize = await _logCleanService.GetDBSize()
            };

            result.EnvsToProjects.AddRange(map.Select(s =>
            {
                var mfEntry = new MapFieldStringEntry()
                {
                    Key = s.Key,
                };
                mfEntry.Value.AddRange(s.Value);

                return mfEntry;
            }));

            return result;
        }
    }
}
