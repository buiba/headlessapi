using EPiServer.ContentApi.Core.Tracking;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Microsoft.Extensions.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.OutputCache.Internal
{
    /// <summary>
    /// Generates an ETag for site definitions
    /// </summary>
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    public class SiteETagGenerator
    {
        /// <summary>
        /// Generates an ETag for site definitions
        /// </summary>
        public virtual string Generate(IEnumerable<ReferencedSiteMetadata > sites)
        {
            var hashCodeCombiner = new HashCodeCombiner();
            foreach (var site in sites.OrderBy(s => s.Id))
            {
                hashCodeCombiner.Add(site.Id);
                if (site.Saved != null)
                {
                    hashCodeCombiner.Add(site.Saved);
                }
            }

            return hashCodeCombiner.CombinedHash.ToString();
        }
    }
}
