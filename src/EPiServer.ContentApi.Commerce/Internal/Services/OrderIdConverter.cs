using EPiServer.ContentApi.Core.Internal;
using System;

namespace EPiServer.ContentApi.Commerce.Internal.Services
{
    public class OrderIdConverter
    {
        private static readonly Guid Namespace = new Guid("96EBCD40-1B0E-46B6-A64B-7446934773F9");
        private GuidEncoder _guidEncoder;

        public OrderIdConverter(GuidEncoder guidEncoder)
        {
            _guidEncoder = guidEncoder;
        }

        public Guid ConverToGuid(int cartId)
        {
            return _guidEncoder.EncodeAsGuid(cartId, Namespace);
        }

        public bool TryConvertToInt(Guid encodedCartId, out int orderId)
        {
            return _guidEncoder.TryDecodeFromGuid(encodedCartId, Namespace, out orderId);
        }
    }
}