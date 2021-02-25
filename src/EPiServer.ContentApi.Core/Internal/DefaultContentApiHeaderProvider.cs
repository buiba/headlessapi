using EPiServer.ServiceLocation;
using System.Collections.Generic;

namespace EPiServer.ContentApi.Core.Internal
{
    [ServiceConfiguration(typeof(IContentApiHeaderProvider))]
    public class DefaultContentApiHeaderProvider : IContentApiHeaderProvider
    {
        public IEnumerable<string> HeaderNames
        {
            get
            {
                yield return "Accept";
                yield return "Accept-Language";
                yield return "x-epi-continuation";
            }
        }
    }
}