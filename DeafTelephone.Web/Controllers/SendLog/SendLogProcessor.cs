namespace DeafTelephone.Controllers.SendLog
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using MediatR;

    using Microsoft.AspNetCore.SignalR;

    using DeafTelephone.Hubs;
    using DeafTelephone.Web.Core.Domain;
    using DeafTelephone.Web.Core.Services;
    using DeafTelephone.Web.Core.Extensions;

    public class SendLogProcessor : IRequestHandler<SendLogQuery>
    {
        internal const string BROADCAST_LOG_MESSAGE_NAME = "BroadcastLog";

        private readonly IHubContext<LogHub> _hubAccess;
        private readonly ILogsStoreService _logStoreService;

        public SendLogProcessor(
            IHubContext<LogHub> hub,
            ILogsStoreService logStoreService)
        {
            _hubAccess = hub ?? throw new ArgumentNullException(nameof(hub));
            _logStoreService = logStoreService ?? throw new ArgumentNullException(nameof(logStoreService));
        }

        public async Task<Unit> Handle(SendLogQuery request, CancellationToken cancellationToken)
        {
            var newRcord = new LogRecord()
            {
                CreatedAt = DateTime.Now,
                LogLevel = (LogLevelEnum)(int)request.Request.Level,
                Message = request.Request.Message.Truncate(255),
                StackTrace = request.Request.StackTrace.Truncate(1024),
                ErrorTitle = request.Request.ErrorTitle.Truncate(255),
                OwnerScopeId = request.Request.OwnerScopeId,
                RootScopeId = request.Request.RootScopeId,
            };

            await _logStoreService.InsertAsync(newRcord);

            // await _hubAccess.Clients.All.SendAsync(BROADCAST_LOG_MESSAGE_NAME, newRcord, cancellationToken);

            return Unit.Value;
        }
    }
}
