namespace DeafTelephone.Web.Controllers.LogiServer.BulkLogOperation
{
    using DeafTelephone.Server;

    using MediatR;

    using System;

    public class BulkLogOperationQuery : IRequest<BulkLogOperationResult>
    {
        public BulkRequest Request { get; init; }

        public BulkLogOperationQuery(BulkRequest request)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
        }
    }
}
