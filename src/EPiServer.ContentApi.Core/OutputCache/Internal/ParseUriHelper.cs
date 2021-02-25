using EPiServer.Core;
using EPiServer.Web;
using System;
using EPiServer.Framework;

namespace EPiServer.ContentApi.Core.OutputCache.Internal
{
    internal static class ParseUriHelper
    {
        /// <summary>
        ///     Try parse content link for request: Get children of a content or get ancestors of a content
        /// </summary>
        /// <param name="uri">Request uri</param>
        /// <param name="permanentLinkMapper">Permanent Link Mapper</param>
        /// <param name="segmentName">segment name to extract the request content link. It would be 
        ///     'children' :  in case getting children
        ///     'ancestors': in case getting ancestors
        /// </param>
        /// <param name="requestContentLink">the requested content link</param>
        public static bool TryParseContentLinkForRequest(Uri uri, IPermanentLinkMapper permanentLinkMapper, string segmentName, out ContentReference requestContentLink)
        {
            Validator.ThrowIfNull(nameof(uri), uri);
            Validator.ThrowIfNull(nameof(permanentLinkMapper), permanentLinkMapper);
            Validator.ThrowIfNullOrEmpty(nameof(segmentName), segmentName);

            requestContentLink = null;
            string contentIdSegment = null;
            for (int i = 0; i < uri.Segments.Length; i++)
            {
                if (uri.Segments[i].Equals(segmentName, StringComparison.OrdinalIgnoreCase))
                {
                    contentIdSegment = uri.Segments[i - 1].TrimEnd('/');
                    break;
                }
            }

            if (contentIdSegment != null)
            {
                if (Guid.TryParse(contentIdSegment, out var contentGuid))
                {
                    var map = permanentLinkMapper.Find(contentGuid);
                    requestContentLink = map?.ContentReference;
                    return requestContentLink != null;
                }
                else if (ContentReference.TryParse(contentIdSegment, out var contentLink))
                {
                    requestContentLink = contentLink;
                    return true;
                }
            }

            return false;
        }
    }
}
