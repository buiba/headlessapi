using EPiServer.ContentApi.Core.Internal;
using System;

namespace EPiServer.ContentApi.Commerce.Internal.Services
{
    public class ShipmentIdConverter
    {
        private static readonly Guid Namespace = new Guid("3827B49D-1EA6-4769-A778-47452044CF04");
        private GuidEncoder _guidEncoder;

        public ShipmentIdConverter(GuidEncoder guidEncoder)
        {
            _guidEncoder = guidEncoder;
        }

        public Guid ConvertToGuid(int cartId)
        {
            return _guidEncoder.EncodeAsGuid(cartId, Namespace);
        }
    }
}
