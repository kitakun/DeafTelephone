namespace DeafTelephone.Web.Hub.Models
{
    using DeafTelephone.Web.Core.Domain;

    using System;

    public class NewScopeEvent
    {
        public const string BROADCAST_NEW_SCOPE_MESSAGE = "BroadcastScope";

        public long Id { get; init; }
        public DateTime CreatedAt { get; init; }

        public long? RootScopeId { get; init; }
        public long? OwnerScopeId { get; init; }

        public string Project { get; init; }
        public string Environment { get; init; }

        public NewScopeEvent(LogScopeRecord source)
        {
            Id = source.Id;
            CreatedAt = source.CreatedAt;
            RootScopeId = source.RootScopeId;
            OwnerScopeId = source.OwnerScopeId;
            Project = source.Project;
            Environment = source.Environment;
        }
    }
}
