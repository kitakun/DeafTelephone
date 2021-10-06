namespace DeafTelephone.Models
{
    using DeafTelephone.Server;

    using System;
    using System.Collections.Generic;

    public class LogModel
    {
        public string Message { get; set; }
        public string ErrorTitle { get; set; }
        public string StackTrace { get; set; }
        public LogLevel Level { get; set; }
        public IDictionary<string, string> Parameters { get; set; }

        public long RootScopeId { get; set; }
        public long OwnerScopeId { get; set; }

        public DateTime CreatedAt { get; set; }

        public LogModel(LogRequest logRequest)
        {
            Message = logRequest.Message;
            Level = logRequest.Level;
            Parameters = logRequest.Parameters;

            RootScopeId = logRequest.RootScopeId;
            OwnerScopeId = logRequest.OwnerScopeId;

            CreatedAt = logRequest.CreatedAt.ToDateTime();
        }

        public LogModel(LogExceptionRequest exceptionLog)
        {
            Message = exceptionLog.Message;
            ErrorTitle = exceptionLog.ExceptionTitle;
            StackTrace = exceptionLog.StackTrace;
            Level = LogLevel.Error;
            Parameters = exceptionLog.Parameters;

            RootScopeId = exceptionLog.RootScopeId;
            OwnerScopeId = exceptionLog.OwnerScopeId;

            CreatedAt = exceptionLog.CreatedAt.ToDateTime();
        }
    }
}
