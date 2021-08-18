﻿namespace DeafTelephone.Web.Controllers.CreateLogScope
{
    using DeafTelephone.Controllers.CreateLogScope;
    using DeafTelephone.Web.Core.Domain;
    using DeafTelephone.Web.Core.Services;

    using MediatR;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class CreateLogScopeProcessor : IRequestHandler<CreateLogScopeQuery, LogScopeRecord>
    {
        private readonly ILogsStoreService _logStoreService;

        public CreateLogScopeProcessor(
            ILogsStoreService logStoreService)
        {
            _logStoreService = logStoreService ?? throw new ArgumentNullException(nameof(logStoreService));
        }

        public async Task<LogScopeRecord> Handle(CreateLogScopeQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var newScope = await _logStoreService.CreateScope(request.Request.RootScopeId, request.Request.OwnerScopeId);

            return newScope;
        }
    }
}
