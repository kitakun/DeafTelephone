namespace DeafTelephone.Controllers.SendLog
{
    using System;

    using DeafTelephone.Models;

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
