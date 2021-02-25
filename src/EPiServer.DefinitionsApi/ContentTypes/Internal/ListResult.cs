using System.Collections;
using System.Collections.Generic;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    internal readonly struct ListResult : IEnumerable<ExternalContentType>
    {
        public ListResult(IEnumerable<ExternalContentType> contentTypes, ContinuationToken continuationToken = default)
        {
            ContentTypes = contentTypes;
            ContinuationToken = continuationToken;
        }

        public readonly IEnumerable<ExternalContentType> ContentTypes;

        public readonly ContinuationToken ContinuationToken;

        public IEnumerator<ExternalContentType> GetEnumerator() => ContentTypes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
