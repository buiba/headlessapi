using EPiServer.ContentApi.Core.Internal;
using System;

namespace EPiServer.ContentApi.Commerce.Internal.Services
{
    public class CartIdConverter
    {
        private static readonly Guid Namespace = new Guid("EC7BC164-14B2-4C76-B2BA-6A83ED8DF451");
        private GuidEncoder _guidEncoder;

        public CartIdConverter(GuidEncoder guidEncoder)
        {
            _guidEncoder = guidEncoder;
        }

        public Guid ConvertToGuid(int cartId)
        {
            return _guidEncoder.EncodeAsGuid(cartId, Namespace);
        }

        public bool TryConvertToInt(Guid encodedCartId, out int cartId)
        {
            return _guidEncoder.TryDecodeFromGuid(encodedCartId, Namespace, out cartId);
        }
    }
}
