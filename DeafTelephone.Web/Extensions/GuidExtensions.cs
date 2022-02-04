namespace DeafTelephone.Web.Extensions
{
    using System;

    using Google.Protobuf;

    public static class GuidExtensions
    {
        public static Guid ToGuid(this ByteString byteString)
        {
            return byteString.IsEmpty
                ? Guid.Empty
                : new Guid(byteString.ToByteArray());
        }
    }
}
