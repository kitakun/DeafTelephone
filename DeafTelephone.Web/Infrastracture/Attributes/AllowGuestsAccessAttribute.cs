namespace DeafTelephone.Web.Infrastracture.Attributes
{
    using System;

    /// <summary>
    /// Method can be called even if no access provided in whitelist
    /// </summary>
    public class AllowGuestsAccessAttribute : Attribute
    {
        public string GrpcServiceNamespace { get; init; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grpcServiceNamespace">grpc namespace to make sure that we allow an access to correct method</param>
        public AllowGuestsAccessAttribute(string grpcServiceNamespace)
        {
            if (string.IsNullOrEmpty(grpcServiceNamespace))
            {
                throw new ArgumentException($"'{nameof(grpcServiceNamespace)}' cannot be null or empty.", nameof(grpcServiceNamespace));
            }

            GrpcServiceNamespace = grpcServiceNamespace;
        }
    }
}
