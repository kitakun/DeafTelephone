namespace DeafTelephone.Web.Infrastracture
{
    using DeafTelephone.Web.Core.Services.Security;

    using Grpc.Core;
    using Grpc.Core.Interceptors;

    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using System.Security;
    using System.Threading.Tasks;

    public class WhitelistInterceptor : Interceptor
    {
        private readonly IWhitelistService _whitelistService;
        private readonly ILogger<WhitelistInterceptor> _logger;
        private readonly IConfiguration _appConfiguration;
        private readonly IMemoryCache _memoryCache;
        private static readonly object mem_api_key = new();

        private const string WHITELIST_API_KEY = "whitelist_api_key";

        public WhitelistInterceptor(
            IWhitelistService whitelistService,
            ILogger<WhitelistInterceptor> logger,
            IConfiguration appConfiguration,
            IMemoryCache memoryCache)
        {
            _whitelistService = whitelistService ?? throw new System.ArgumentNullException(nameof(whitelistService));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            _appConfiguration = appConfiguration ?? throw new System.ArgumentNullException(nameof(appConfiguration));
            _memoryCache = memoryCache ?? throw new System.ArgumentNullException(nameof(memoryCache));
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            // validate access by API Key
            var apiAccess = context.RequestHeaders.GetValue(WHITELIST_API_KEY);
            if (!string.IsNullOrEmpty(apiAccess))
            {
                var keyFromConfig = _memoryCache.GetOrCreate(mem_api_key, (entry) =>
                {
                    return _appConfiguration["WhitelistKey"];
                });

                if (!string.IsNullOrEmpty(keyFromConfig) && keyFromConfig.Equals(apiAccess, System.StringComparison.OrdinalIgnoreCase))
                {
                    return await continuation(request, context);
                }
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            // validate access by WhiteList API list
            var httpContext = context.GetHttpContext();
            var clientIP = httpContext.Connection.RemoteIpAddress;
            var address = clientIP.ToString();

            const string ipv6Submask = "::ffff:";
            if (address.Length > 6 && address.Substring(0, ipv6Submask.Length) == ipv6Submask)
            {
                address = address[ipv6Submask.Length..];
            }

            var isAllowed = await _whitelistService.IsAllowedAsync(address);
            if (isAllowed)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                return await continuation(request, context);
            }

            _logger.LogWarning($"Client with address={address} tried to connect, but we refused him");

            throw new SecurityException("Not allowed");
        }
    }
}
