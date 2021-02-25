using EPiServer.ContentApi.Core.Internal;
using System;

namespace EPiServer.ContentApi.Commerce.Internal.Services
{
    public class LineItemIdConverter
    {
        private static readonly Guid Namespace = new Guid("A083F095-736B-4987-AA2D-7B55DFB1E2A0");
        private GuidEncoder _guidEncoder;

        public LineItemIdConverter(GuidEncoder guidEncoder)
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
