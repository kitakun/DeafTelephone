namespace DeafTelephone
{
    using System;
    using System.Threading.Tasks;

    using Grpc.Core;
    using Microsoft.Extensions.Logging;
    using MediatR;

    using DeafTelephone.Server;
    using DeafTelephone.Controllers.SendLog;
    using DeafTelephone.Controllers.CreateLogScope;

    public class LogiService : Logger.LoggerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<LogiService> _logger;

        public LogiService(
            IMediator mediator,
            ILogger<LogiService> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ping

        public override Task<PongReply> Ping(PingRequest request, ServerCallContext context)
        {
            return Task.FromResult(new PongReply());
        }

        // logs

        public override async Task<LogResponse> Log(LogRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation($"Recieved msg={request.Message} for scope={request.OwnerScopeId}");

                await _mediator.Send(new SendLogQuery(new Models.LogModel(request)));

                return new LogResponse
                {
                    IsSuccess = true
                };
            }
            catch (Exception es)
            {
                _logger.LogError($"Recieved msg={request.Message} for scope={request.OwnerScopeId} but got error={es.Message} stack={es.StackTrace}");

                return new LogResponse
                {
                    IsSuccess = false,
                    Error = es.Message
                };
            }
        }

        public override async Task<LogResponse> LogException(LogExceptionRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation($"Recieved msg={request.Message} for scope={request.OwnerScopeId}");

                await _mediator.Send(new SendLogQuery(new Models.LogModel(request)));

                return new LogResponse
                {
                    IsSuccess = true
                };
            }
            catch (Exception es)
            {
                _logger.LogError($"Recieved msg={request.Message} for scope={request.OwnerScopeId} but got error={es.Message} stack={es.StackTrace}");

                return new LogResponse
                {
                    IsSuccess = false,
                    Error = es.Message
                };
            }
        }

        // scope

        public override async Task<BeginScopeResponse> BeginScope(BeginScopeRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation($"Recieved {nameof(BeginScope)} for scope={request.OwnerScopeId}");

                var createdScope = await _mediator.Send(new CreateLogScopeQuery(request));

                return new BeginScopeResponse
                {
                    ScopeId = createdScope.Id
                };
            }
            catch (Exception es)
            {
                _logger.LogError($"Recieved {nameof(BeginScope)} for scope={request.OwnerScopeId} but got error={es.Message} stack={es.StackTrace}");

                return new BeginScopeResponse
                {
                    Error = es.Message
                };
            }
        }

        // bulk

        public override Task<BulkRespons> Bulk(BulkRequest request, ServerCallContext context)
        {
            return base.Bulk(request, context);
        }
    }
}
