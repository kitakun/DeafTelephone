namespace DeafTelephone.Controllers.LogiServer.SendLog
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
    using DeafTelephone.Web.Hub.Models;

    public class SendLogProcessor : IRequestHandler<SendLogQuery>
    {
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

            await _hubAccess.Clients.Group(LogHub.ALL_LOGS_GROUP).SendAsync(
                            NewLogInScopeEvent.BROADCAST_LOG_MESSAGE_NAME, new NewLogInScopeEvent(newRcord), cancellationToken);

            return Unit.Value;
        }
    }
}
