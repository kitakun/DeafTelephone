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
                Message = request.Request.Message,
                StackTrace = request.Request.StackTrace,
                ErrorTitle = request.Request.ErrorTitle,
                OwnerScopeId = request.Request.OwnerScopeId,
                RootScopeId = request.Request.RootScopeId,
            };

            var hubNotifyTask = _hubAccess.Clients.All.SendAsync(BROADCAST_LOG_MESSAGE_NAME, request.Request, cancellationToken);
            var storeLogTask = _logStoreService.InsertAsync(newRcord);

            await Task.WhenAll(hubNotifyTask, storeLogTask);

            return Unit.Value;
        }
    }
}
