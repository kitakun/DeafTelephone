namespace DeafTelephone.Web.Infrastracture
{
    using DeafTelephone.Web.Core.Services.Security;

    using Grpc.Core;
    using Grpc.Core.Interceptors;

    using Microsoft.Extensions.Logging;

    using System.Security;
    using System.Threading.Tasks;

    public class WhitelistInterceptor : Interceptor
    {
        private readonly IWhitelistService _whitelistService;
        private readonly ILogger<WhitelistInterceptor> _logger;

        public WhitelistInterceptor(
            IWhitelistService whitelistService,
            ILogger<WhitelistInterceptor> logger)
        {
            _whitelistService = whitelistService ?? throw new System.ArgumentNullException(nameof(whitelistService));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var httpContext = context.GetHttpContext();
            var clientIP = httpContext.Connection.RemoteIpAddress;
            var address = clientIP.ToString();

            const string ipv6Submask = "::ffff:";
            if (address.Substring(0, ipv6Submask.Length) == ipv6Submask)
            {
                address = address[ipv6Submask.Length..];
            }

            var isAllowed = await _whitelistService.IsAllowedAsync(address);
            if (isAllowed)
            {
                return await continuation(request, context);
            }

            _logger.LogWarning($"Client with address={address} tried to connect, but we refused him");

            throw new SecurityException("Not allowed");
        }
    }
}
