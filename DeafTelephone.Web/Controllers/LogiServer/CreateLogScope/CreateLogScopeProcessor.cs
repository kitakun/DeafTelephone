namespace DeafTelephone.Web.Controllers.LogiServer.CreateLogScope
{
    using DeafTelephone.Controllers.LogiServer.CreateLogScope;
    using DeafTelephone.Hubs;
    using DeafTelephone.Web.Core.Domain;
    using DeafTelephone.Web.Core.Services;
    using DeafTelephone.Web.Hub.Models;

    using MediatR;

    using Microsoft.AspNetCore.SignalR;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class CreateLogScopeProcessor : IRequestHandler<CreateLogScopeQuery, LogScopeRecord>
    {
        private readonly ILogsStoreService _logStoreService;
        private readonly IHubContext<LogHub> _hub;

        public CreateLogScopeProcessor(
            ILogsStoreService logStoreService,
            IHubContext<LogHub> hub)
        {
            _logStoreService = logStoreService ?? throw new ArgumentNullException(nameof(logStoreService));
            _hub = hub ?? throw new ArgumentNullException(nameof(hub));
        }

        public async Task<LogScopeRecord> Handle(CreateLogScopeQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var newScope = await _logStoreService.CreateScope(
                request.Request.RootScopeId,
                request.Request.OwnerScopeId,
                request.Request.CreatedAt.ToDateTime());

            await _hub
                .Clients
                .Group(LogHub.ALL_LOGS_GROUP).
                SendAsync(NewScopeEvent.BROADCAST_NEW_SCOPE_MESSAGE, new NewScopeEvent(newScope), cancellationToken);

            return newScope;
        }
    }
}
