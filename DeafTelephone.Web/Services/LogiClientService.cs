namespace DeafTelephone.Web.Services
{
    using ForClient;
    using Controllers.LogiClient.Fetch;
    using Controllers.LogiClient.Hello;
    using Infrastracture.Attributes;

    using Grpc.Core;

    using MediatR;

    using System.Threading.Tasks;

    public class LogiClientService : LoggerClient.LoggerClientBase
    {
        private readonly IMediator _mediator;

        public LogiClientService(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        public override Task<PongReply> Ping(PingRequest request, ServerCallContext context)
        {
            return Task.FromResult(new PongReply());
        }

        /// <summary>
        /// Get list of { Envs+Projects[] }
        /// </summary>
        [AllowGuestsAccess("logiClient.LoggerClient")]
        public override async Task<HelloResponse> Hello(HelloRequest request, ServerCallContext context)
        {
            return await _mediator.Send(new HelloQuery(request));
        }

        /// <summary>
        /// Fetch logs list
        /// </summary>
        public override async Task<FetchLogResponse> Fetch(FetchLogRequest request, ServerCallContext context)
        {
            return await _mediator.Send(new FetchQuery(request, context.CancellationToken));
        }
    }
}
