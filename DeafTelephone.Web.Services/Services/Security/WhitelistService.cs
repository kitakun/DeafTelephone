namespace DeafTelephone.Web.Services.Services.Security
{
    using DeafTelephone.Web.Core.Services.Security;
    using DeafTelephone.Web.Services.Persistence;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;

    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal class WhitelistService : IWhitelistService
    {
        private static readonly object whitelist_key = new();

        private readonly LogDbContext _dbContext;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _appConfigs;

        public WhitelistService(
            LogDbContext dbContext,
            IMemoryCache cache,
            IConfiguration appConfigs)
        {
            _dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext));
            _cache = cache ?? throw new System.ArgumentNullException(nameof(cache));
            _appConfigs = appConfigs ?? throw new System.ArgumentNullException(nameof(appConfigs));
        }

        public async ValueTask<bool> IsAllowedAsync(string address)
        {
            var allowedIPs = await _cache.GetOrCreateAsync(whitelist_key, async (entry) =>
            {
                var listFromConfig = _appConfigs.GetSection("Whitelist")
                    .GetChildren()
                    .ToList()
                    .Select(s => s.Value)
                    .ToList();

                var fromDb = await _dbContext
                    .Settigns
                    .AsNoTracking()
                    .Where(w => w.Type == Core.Domain.SettingRecordEnum.AllowedIP)
                    .Select(s => s.Value)
                    .Distinct()
                    .ToListAsync();

                return fromDb.Union(listFromConfig).ToArray();
            });

            return allowedIPs.Contains(address);
        }

        public void ClearCache()
        {
            _cache.Remove(whitelist_key);
        }
    }
}
