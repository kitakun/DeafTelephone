namespace DeafTelephone.Web.Core.Domain
{
    using System;
    using System.Collections.Generic;

    public record LogScopeRecord
    {
        public long Id { get; init; }
        public DateTime CreatedAt { get; init; }

        public long? RootScopeId { get; init; }
        public long? OwnerScopeId { get; init; }

        public virtual ICollection<LogRecord> InnerLogsCollection { get; init; }
    }
}
