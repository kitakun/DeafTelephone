namespace DeafTelephone
{
    using System;
    using System.Threading.Tasks;

    using Grpc.Core;

    using MediatR;

    using DeafTelephone.Controllers.SendLog;

    public class LogiService : Logger.LoggerBase
    {
        private readonly IMediator _mediator;

        public LogiService(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "PingPong"
            });
        }

        public override async Task<LogResponse> Log(LogRequest request, ServerCallContext context)
        {
            try
            {
                await _mediator.Send(new SendLogQuery(new Models.LogModel(request)));
                return new LogResponse
                {
                    IsSuccess = true
                };
            }
            catch (Exception es)
            {
                return new LogResponse
                {
                    IsSuccess = false,
                    Error = es.Message
                };
            }
        }

        public override async Task<LogResponse> LogException(LogExceptionRequest request, ServerCallContext context)
        {
            try
            {
                await _mediator.Send(new SendLogQuery(new Models.LogModel(request)));
                return new LogResponse
                {
                    IsSuccess = true
                };
            }
            catch (Exception es)
            {
                return new LogResponse
                {
                    IsSuccess = false,
                    Error = es.Message
                };
            }
        }
    }
}
