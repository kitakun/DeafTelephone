namespace DeafTelephone.Web.Controllers.LogiServer.IncomplitedScope
{
    using MediatR;

    public class IncomplitedScopeQuery : IRequest
    {
        public string CacheKey { get; init; }

        public IncomplitedScopeQuery(string cacheKey)
        {
            CacheKey = cacheKey;
        }
    }
}
