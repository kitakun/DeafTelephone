namespace DeafTelephone.Web.Extensions
{
    using System;

    using Google.Protobuf;

    public static class GuidExtensions
    {
        public static Guid ToGuid(this ByteString byteString)
        {
            if (byteString.IsEmpty)
            {
                return Guid.Empty;
            }
            return new Guid(byteString.ToByteArray());
        }
    }
}
