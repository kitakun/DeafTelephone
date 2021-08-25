using DeafTelephone.Web.Services.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DeafTelephone.Web.Controllers.LogiClient.Hello
{
    public class HelloProcessor : IRequestHandler<HelloQuery, HelloResult>
    {
        private readonly LogDbContext _dbContext;

        public HelloProcessor(LogDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext));
        }

        public async Task<HelloResult> Handle(HelloQuery request, CancellationToken cancellationToken)
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

            return new HelloResult(map);
        }
    }
}
