namespace DeafTelephone.Web.Extensions
{
    using Microsoft.AspNetCore.Http;

    public static class WebExtensions
    {
        public static string GetIPAddress(this HttpContext context)
        {
            var clientIP = context.Connection.RemoteIpAddress;
            var address = clientIP.ToString();

            const string ipv6Submask = "::ffff:";
            if (address.Length > 6 && address.Substring(0, ipv6Submask.Length) == ipv6Submask)
            {
                address = address[ipv6Submask.Length..];
            }

            return address;
        }
    }
}
