namespace DeafTelephone.Web.Hub.Models
{
    using DeafTelephone.Web.Core.Domain;

    using System;

    public class NewLogInScopeEvent
    {
        public const string BROADCAST_LOG_MESSAGE_NAME = "BroadcastLog";

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

        public NewLogInScopeEvent(LogRecord source)
        {
            Id = source.Id;
            CreatedAt = source.CreatedAt;

            LogLevel = source.LogLevel;

            Message = source.Message;

            ErrorTitle = source.ErrorTitle;
            StackTrace = source.StackTrace;

            RootScopeId = source.RootScopeId;
            OwnerScopeId = source.OwnerScopeId;
        }
    }
}
