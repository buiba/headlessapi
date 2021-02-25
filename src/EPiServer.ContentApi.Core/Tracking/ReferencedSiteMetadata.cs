using Microsoft.Extensions.Internal;
using System;

namespace EPiServer.ContentApi.Core.Tracking
{
    /// <summary>
    /// Metadata for a site
    /// </summary>
    public class ReferencedSiteMetadata : IEquatable<ReferencedSiteMetadata>
    {
        /// <summary>
        /// This plays a role as a common cache key that can be used to evict etag (for get list of site) when a site is added/updated/deleted
        /// </summary>
        public static ReferencedSiteMetadata DefaultInstance = new ReferencedSiteMetadata(Guid.Parse("173C13B5-DE77-4B2A-9F5E-43273DAD773C"), null);

        public ReferencedSiteMetadata(Guid id, DateTime? saved)
        {
            Id = id;
            Saved = saved;
        }

        /// <summary>
        /// Site Id
        /// </summary>
        public Guid Id;

        /// <summary>
        /// The recent time that the site has been updated
        /// </summary>
        public DateTime? Saved;

        /// <summary>
        /// Get hashcode for the class for comparison
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var hashCodeCombiner = new HashCodeCombiner();
            hashCodeCombiner.Add(Id);
            if (this.Saved != null)
            {
                hashCodeCombiner.Add(Saved);
            }
            return hashCodeCombiner.CombinedHash;
        }

        /// <summary>
        /// Compare the current object with another object
        /// </summary>
        public bool Equals(ReferencedSiteMetadata other)
        {
            if (other == null) return false;
            return GetHashCode() == other.GetHashCode();
        }
    }
}
