namespace DeafTelephone.Web.Controllers.LogiClient.Fetch
{
    using DeafTelephone.ForClient;
    using DeafTelephone.Web.Core.Domain;
    using DeafTelephone.Web.Core.Services;

    using LinqKit;

    using MediatR;

    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class FetchProcessor : IRequestHandler<FetchQuery, FetchLogResponse>
    {
        private readonly ILogsStoreService _logStoreService;

        public FetchProcessor(
            ILogsStoreService logStoreService)
        {
            _logStoreService = logStoreService ?? throw new System.ArgumentNullException(nameof(logStoreService));
        }

        public async Task<FetchLogResponse> Handle(FetchQuery request, CancellationToken cancellationToken)
        {
            var logPredicate = PredicateBuilder.New<LogRecord>(true);
            var rootScopePredicate = PredicateBuilder.New<LogScopeRecord>(true);

            if (!string.IsNullOrEmpty(request.Request.Query))
            {
                logPredicate = logPredicate.And(w => w.Message.Contains(request.Request.Query) || w.ErrorTitle.Contains(request.Request.Query));
            }

            if(request.Request.Projects.Count > 0)
            {
                var selectedProjectsList = request.Request.Projects.ToList();
                rootScopePredicate = rootScopePredicate.And(w => selectedProjectsList.Contains(w.Project.ToLower()));
            }

            if (request.Request.Enves.Count > 0)
            {
                var selectedEnvsList = request.Request.Enves.ToList();
                rootScopePredicate = rootScopePredicate.And(w => selectedEnvsList.Contains(w.Environment.ToLower()));
            }

            var (scopes, logs) = await _logStoreService.Fetch(request.Request.From, logPredicate, rootScopePredicate);

            // create response

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
                    Level = (LogLevel)(int)dbModel.LogLevel,
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
