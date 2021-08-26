namespace DeafTelephone.Web.Controllers.LogiClient.Fetch
{
    using DeafTelephone.ForClient;

    using MediatR;

    using System;
    using System.Threading;

    public class FetchQuery : IRequest<FetchLogResponse>
    {
        public FetchLogRequest Request { get; init; }
        public CancellationToken CancellationToken { get; init; }

        public FetchQuery(FetchLogRequest request, CancellationToken cancellationToken)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
            CancellationToken = cancellationToken;
        }
    }
}
