namespace DeafTelephone.Web.Jobs
{
    using System.Threading.Tasks;

    public interface IScopedJob
    {
        Task Launch();
    }
}
