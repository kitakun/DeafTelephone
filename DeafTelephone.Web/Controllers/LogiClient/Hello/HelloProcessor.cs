namespace DeafTelephone.Web.Controllers.LogiClient.Hello
{
    using DeafTelephone.ForClient;
    using DeafTelephone.Web.Core.Services;
    using DeafTelephone.Web.Core.Services.Security;
    using DeafTelephone.Web.Extensions;
    using DeafTelephone.Web.Services.Persistence;

    using MediatR;

    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    public class HelloProcessor : IRequestHandler<HelloQuery, HelloResponse>
    {
        private readonly LogDbContext _dbContext;
        private readonly ILogCleanerService _logCleanService;
        private readonly IWhitelistService _whitelistService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HelloProcessor(
            LogDbContext dbContext,
            ILogCleanerService logCleanService,
            IWhitelistService whitelistService,
            IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext));
            _logCleanService = logCleanService ?? throw new System.ArgumentNullException(nameof(logCleanService));
            _whitelistService = whitelistService ?? throw new System.ArgumentNullException(nameof(whitelistService));
            _httpContextAccessor = httpContextAccessor ?? throw new System.ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<HelloResponse> Handle(HelloQuery request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = httpContext.GetIPAddress();
            var hasAnAccess = await _whitelistService.IsAllowedAsync(ipAddress);

            if (hasAnAccess)
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
            else
            {
                // say to client, that he doesn't allowed to be here
                return new HelloResponse()
                {
                    Error = nameof(HttpStatusCode.Forbidden)
                };
            }
        }
    }
}
