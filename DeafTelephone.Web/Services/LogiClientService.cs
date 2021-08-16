﻿namespace DeafTelephone.Web.Services
{
    using System;
    using System.Threading.Tasks;

    using Grpc.Core;
    using Microsoft.Extensions.Logging;
    using MediatR;

    using DeafTelephone.Controllers.SendLog;
    using DeafTelephone.Controllers.CreateLogScope;
    using DeafTelephone.ForClient;
    using DeafTelephone.Web.Core.Services;

    public class LogiClientService : LoggerClient.LoggerClientBase
    {
        private readonly ILogsStoreService _logStoreService;

        public LogiClientService(ILogsStoreService logStoreService)
        {
            _logStoreService = logStoreService;
        }

        public override Task<PongReply> Ping(PingRequest request, ServerCallContext context)
        {
            return Task.FromResult(new PongReply());
        }

        public override async Task<FetchLogResponse> Fetch(FetchLogRequest request, ServerCallContext context)
        {
            var (scopes, logs) = await _logStoreService.Fetch();

            var response = new FetchLogResponse()
            {
                IsSuccess = true,
            };

            foreach (var dbModel in logs)
            {
                response.Logs.Add(new LogMessage
                {
                    LogId = dbModel.Id,
                    ExceptionTitle = string.IsNullOrEmpty(dbModel.ErrorTitle) ? string.Empty : dbModel.ErrorTitle,
                    Level = (DeafTelephone.ForClient.LogLevel)(int)dbModel.LogLevel,
                    StackTrace = string.IsNullOrEmpty(dbModel.StackTrace) ? string.Empty : dbModel.StackTrace,
                    OwnerScopeId = dbModel.OwnerScopeId ?? 0,
                    Message = string.IsNullOrEmpty(dbModel.Message) ? string.Empty : dbModel.Message,
                    CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(dbModel.CreatedAt.ToUniversalTime())
                });
            }

            foreach(var scope in scopes)
            {
                response.Scopes.Add(new LogScope()
                {
                    CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(scope.CreatedAt.ToUniversalTime()),
                    OwnerScopeId = scope.OwnerScopeId ?? -1,
                    RootScopeId = scope.RootScopeId ?? -1,
                    ScopeId = scope.Id
                });
            }

            return response;
        }
    }
}
