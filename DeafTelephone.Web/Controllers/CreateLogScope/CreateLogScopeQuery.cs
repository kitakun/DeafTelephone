namespace DeafTelephone.Controllers.CreateLogScope
{
    using DeafTelephone.Server;
    using DeafTelephone.Web.Core.Domain;

    using MediatR;

    using System;

    public class CreateLogScopeQuery : IRequest<LogScopeRecord>
    {
        public BeginScopeRequest Request { get; init; }

        /// <summary>
        /// Create log scope
        /// </summary>
        public CreateLogScopeQuery(BeginScopeRequest request)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
        }
    }
}
