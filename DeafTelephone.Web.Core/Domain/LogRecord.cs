namespace DeafTelephone.Web.Core.Domain
{
    using System;

    public record LogRecord()
    {
        public long Id { get; init; }
        public DateTime CreatedAt { get; init; }

        public LogLevelEnum LogLevel { get; init; }

        // default msg data
        public string Message { get; init; }

        // exception fields
        public string ErrorTitle { get; init; }
        public string StackTrace { get; init; }

        public long? RootScopeId { get; init; }

        public long? OwnerScopeId { get; init; }
        public virtual LogScopeRecord OwnerScope { get; init; }
    };
}
