namespace DeafTelephone.Controllers.LogiServer.SendLog
{
    using System;

    using Models;

    using MediatR;

    public class SendLogQuery : IRequest
    {
        public LogModel Request { get; init; }

        /// <summary>
        /// Send log to all listners (JS)
        /// </summary>
        public SendLogQuery(LogModel request)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
        }
    }
}
