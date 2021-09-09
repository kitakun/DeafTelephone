namespace DeafTelephone.Web.Infrastracture
{
    using DeafTelephone.Web.Core.Services.Security;
    using DeafTelephone.Web.Extensions;
    using DeafTelephone.Web.Infrastracture.Attributes;

    using Grpc.Core;
    using Grpc.Core.Interceptors;

    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using System;
    using System.Linq;
    using System.Reflection;
    using System.Security;
    using System.Threading.Tasks;

    public class WhitelistInterceptor : Interceptor
    {
        private static readonly (string className, string methodName)[] IgnoredMethodsToBeProtected;

        static WhitelistInterceptor()
        {
            IgnoredMethodsToBeProtected = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(AllowGuestsAccessAttribute), false).Length > 0)
                .Select(s => (s.GetCustomAttribute<AllowGuestsAccessAttribute>().GrpcServiceNamespace, s.Name))
                .ToArray();
        }

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
            _whitelistService = whitelistService ?? throw new ArgumentNullException(nameof(whitelistService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appConfiguration = appConfiguration ?? throw new ArgumentNullException(nameof(appConfiguration));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var splittedMethodInfo = context.Method.Split("/");
            var method = splittedMethodInfo[^1];
            var service = splittedMethodInfo[^2];
            if (IgnoredMethodsToBeProtected.Any(a => a.methodName == method && a.className == service))
            {
                return await continuation(request, context);
            }

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

            var httpContext = context.GetHttpContext();
            var ipAddress = httpContext.GetIPAddress();

            // validate access by WhiteList API list
            var isAllowed = await _whitelistService.IsAllowedAsync(ipAddress);
            if (isAllowed)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                return await continuation(request, context);
            }

            _logger.LogWarning($"Client with address={ipAddress} tried to connect, but we refused him");

            throw new SecurityException("Not allowed");
        }
    }
}
