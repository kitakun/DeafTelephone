namespace DeafTelephone.Web.Core.Domain
{
    using System;

    public record LogScopeRecord
    {
        public int Id { get; init; }
        public DateTime CreatedAt { get; init; }

        public int OwnerScopeId { get; init; }
    }
}
