namespace DeafTelephone.Web.Core.Models
{
    using DeafTelephone.Web.Core.Domain;

    using System;
    using System.Linq.Expressions;

    public readonly struct LogFetchFilters
    {
        public readonly int? SkipScopes;
        public readonly int? TakeScopes;
        public readonly Expression<Func<LogRecord, bool>> PredicateLogQuery;
        public readonly Expression<Func<LogScopeRecord, bool>> PredicateRootScopeQuery;

        public LogFetchFilters(
            int from,
            int take,
            Expression<Func<LogRecord, bool>> logPredicate,
            Expression<Func<LogScopeRecord, bool>> rootScopePredicate) : this()
        {
            SkipScopes = from;
            TakeScopes = take > 0
                    ? take
                    : (int?)null;
            PredicateLogQuery = logPredicate;
            PredicateRootScopeQuery = rootScopePredicate;
        }
    }
}
