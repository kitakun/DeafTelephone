namespace DeafTelephone.Web.Hub.Hubs
{
    using System.Threading.Tasks;

    public interface ILogHub
    {
        /// <summary>
        /// Is client allowed to listen SignalR messages
        /// </summary>
        /// <returns>true or false</returns>
        ValueTask<bool> IsAllowed();
    }
}
