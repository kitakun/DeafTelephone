namespace DeafTelephone.Web.Controllers.LogiClient.Hello
{
    using DeafTelephone.ForClient;

    using MediatR;

    public class HelloQuery : IRequest<HelloResponse>
    {
        public HelloRequest Request { get; init;  }

        public HelloQuery(HelloRequest request)
        {
            Request = request ?? throw new System.ArgumentNullException(nameof(request));
        }
    }
}
