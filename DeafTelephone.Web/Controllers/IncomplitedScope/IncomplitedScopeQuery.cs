namespace DeafTelephone.Web.Controllers.IncomplitedScope
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
