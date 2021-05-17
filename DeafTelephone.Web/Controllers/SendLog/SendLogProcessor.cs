namespace DeafTelephone.Controllers.SendLog
{
    using System.Threading;
    using System.Threading.Tasks;

    using DeafTelephone.Hubs;

    using MediatR;

    using Microsoft.AspNetCore.SignalR;

    public class SendLogProcessor : IRequestHandler<SendLogQuery>
    {
        internal const string BROADCAST_LOG_MESSAGE_NAME = "BroadcastLog";

        private readonly IHubContext<LogHub> _hubAccess;

        public SendLogProcessor(IHubContext<LogHub> hub)
        {
            _hubAccess = hub ?? throw new System.ArgumentNullException(nameof(hub));
        }

        public async Task<Unit> Handle(SendLogQuery request, CancellationToken cancellationToken)
        {
            await _hubAccess.Clients.All.SendAsync(BROADCAST_LOG_MESSAGE_NAME, request.Request, cancellationToken);

            return Unit.Value;
        }
    }
}
