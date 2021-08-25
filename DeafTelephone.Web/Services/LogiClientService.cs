namespace DeafTelephone.Web.Services
{
    using DeafTelephone.ForClient;
    using DeafTelephone.Web.Controllers.LogiClient.Hello;
    using DeafTelephone.Web.Core.Services;

    using Grpc.Core;

    using MediatR;

    using System.Linq;
    using System.Threading.Tasks;

    public class LogiClientService : LoggerClient.LoggerClientBase
    {
        private readonly IMediator _mediator;
        private readonly ILogsStoreService _logStoreService;

        public LogiClientService(
            IMediator mediator,
            ILogsStoreService logStoreService)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
            _logStoreService = logStoreService ?? throw new System.ArgumentNullException(nameof(logStoreService));
        }

        public override Task<PongReply> Ping(PingRequest request, ServerCallContext context)
        {
            return Task.FromResult(new PongReply());
        }

        /// <summary>
        /// Get list of { Envs+Projects[] }
        /// </summary>
        public override async Task<HelloResponse> Hello(HelloRequest request, ServerCallContext context)
        {
            var methodResult = await _mediator.Send(new HelloQuery(request));

            var result = new HelloResponse();

            result.EnvsToProjects.AddRange(methodResult.EnvsToProjectsMap.Select(s =>
            {
                var mfEntry = new MapFieldStringEntry()
                {
                    Key = s.Key,
                };
                mfEntry.Value.AddRange(s.Value);

                return mfEntry;
            }));

            return result;
        }

        public override async Task<FetchLogResponse> Fetch(FetchLogRequest request, ServerCallContext context)
        {
            // TODO to MediatR
            var (scopes, logs) = string.IsNullOrEmpty(request.Query)
                ? await _logStoreService.Fetch(request.From)
                : await _logStoreService.Fetch(request.From, request.Query);

            var response = new FetchLogResponse()
            {
                IsSuccess = true,
            };

            foreach (var dbModel in logs)
            {
                response.Logs.Add(new LogMessage
                {
                    LogId = dbModel.Id,
                    ExceptionTitle = string.IsNullOrEmpty(dbModel.ErrorTitle) ? string.Empty : dbModel.ErrorTitle,
                    Level = (DeafTelephone.ForClient.LogLevel)(int)dbModel.LogLevel,
                    StackTrace = string.IsNullOrEmpty(dbModel.StackTrace) ? string.Empty : dbModel.StackTrace,
                    OwnerScopeId = dbModel.OwnerScopeId ?? 0,
                    Message = string.IsNullOrEmpty(dbModel.Message) ? string.Empty : dbModel.Message,
                    CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(dbModel.CreatedAt.ToUniversalTime())
                });
            }

            foreach (var scope in scopes)
            {
                response.Scopes.Add(new LogScope()
                {
                    CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(scope.CreatedAt.ToUniversalTime()),
                    OwnerScopeId = scope.OwnerScopeId ?? -1,
                    RootScopeId = scope.RootScopeId ?? -1,
                    ScopeId = scope.Id
                });
            }

            return response;
        }
    }
}
