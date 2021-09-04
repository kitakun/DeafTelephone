namespace DeafTelephone.Web.Controllers.LogiServer.IncomplitedScope
{
    using MediatR;

    public class IncomplitedScopeQuery : IRequest
    {
        public string CacheKey { get; init; }
        public string Message { get; init; }
        public string StackTrace { get; init; }

        public IncomplitedScopeQuery(
            string cacheKey,
            string message,
            string stacktrace)
        {
            CacheKey = cacheKey;
            Message = message;
            StackTrace = stacktrace;
        }
    }
}
