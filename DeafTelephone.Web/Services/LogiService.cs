namespace DeafTelephone
{
    using Controllers.LogiServer.CreateLogScope;
    using Controllers.LogiServer.SendLog;
    using Server;
    using DeafTelephone.Web.Controllers.LogiServer.BulkLogOperation;
    using DeafTelephone.Web.Controllers.LogiServer.IncomplitedScope;

    using Grpc.Core;

    using MediatR;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Threading.Tasks;

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
                _logger.LogInformation($"Received msg={request.Message} for scope={request.OwnerScopeId}");

                await _mediator.Send(new SendLogQuery(new Models.LogModel(request)));

                return new LogResponse
                {
                    IsSuccess = true
                };
            }
            catch (Exception es)
            {
                _logger.LogError($"Received msg={request.Message} for scope={request.OwnerScopeId} but got error={es.Message} stack={Environment.NewLine}{es.StackTrace}");

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
                _logger.LogInformation($"Received msg={request.Message} for scope={request.OwnerScopeId}");

                await _mediator.Send(new SendLogQuery(new Models.LogModel(request)));

                return new LogResponse
                {
                    IsSuccess = true
                };
            }
            catch (Exception es)
            {
                _logger.LogError($"Received msg={request.Message} for scope={request.OwnerScopeId} but got error={es.Message} stack={Environment.NewLine}{es.StackTrace}");

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
                _logger.LogInformation($"Received {nameof(BeginScope)} for scope={request.OwnerScopeId}");

                var createdScope = await _mediator.Send(new CreateLogScopeQuery(request));

                return new BeginScopeResponse
                {
                    ScopeId = createdScope.Id
                };
            }
            catch (Exception es)
            {
                _logger.LogError($"Received {nameof(BeginScope)} for scope={request.OwnerScopeId} but got error={es.Message} stack={Environment.NewLine}{es.StackTrace}");

                return new BeginScopeResponse
                {
                    Error = es.Message
                };
            }
        }

        // bulk

        public override async Task<BulkRespons> Bulk(BulkRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation($"Received {nameof(Bulk)} with messages count={request.Messages.Count}");

                var createdScope = await _mediator.Send(new BulkLogOperationQuery(request));

                return new BulkRespons
                {
                    IsSuccess = true,
                    CacheKey = createdScope.CacheKey
                };
            }
            catch (Exception es)
            {
                _logger.LogError($"Received {nameof(Bulk)} with messages count={request.Messages.Count} but got error={es.Message} stack={Environment.NewLine}{es.StackTrace}");

                return new BulkRespons
                {
                    Error = es.Message,
                    IsSuccess = false
                };
            }
        }

        // utils

        public override async Task<IncomplitedScopeResponse> IncomplitedRequest(IncomplitedScopeReqest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation($"Received {nameof(IncomplitedRequest)} for Key={request.CacheKey}");

                await _mediator.Send(new IncomplitedScopeQuery(request.CacheKey, request.Message, request.Stacktrace));
            }
            catch (Exception es)
            {
                _logger.LogError(
                "Received {IncomplitedScopeResponse} for Key={CacheKey} but got error={Message} stack={NewLine}{StackTrace}",
                 nameof(IncomplitedScopeResponse),
                    request.CacheKey,
                    es.Message,
                    Environment.NewLine,
                    es.StackTrace);
            }

            return new IncomplitedScopeResponse();
        }
    }
}
