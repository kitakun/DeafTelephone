namespace DeafTelephone.Web.Jobs
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IScopedJob
    {
        Task LaunchAsync(CancellationToken token);
    }
}
