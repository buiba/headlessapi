using EPiServer.Core;
using Microsoft.Extensions.Internal;
using System;
using System.Globalization;

namespace EPiServer.ContentApi.Core.Tracking
{
    /// <summary>
    /// Represent a reference to a content item in a specific language
    /// </summary>
    public class LanguageContentReference : IEquatable<LanguageContentReference>
    {
        /// <summary>
        /// Creates a new instance of <see cref="LanguageContentReference"/>
        /// </summary>
        public LanguageContentReference(ContentReference contentLink, CultureInfo language)
        {
            ContentLink = contentLink;
            Language = language ?? CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Reference to a content
        /// </summary>
        public ContentReference ContentLink { get; }

        /// <summary>
        /// The language of the content
        /// </summary>
        public CultureInfo Language { get; }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCodeCombiner = new HashCodeCombiner();
            hashCodeCombiner.Add(ContentLink);
            hashCodeCombiner.Add(Language);
            return hashCodeCombiner.CombinedHash;
        }

        /// <summary>
        /// Indicates whether the current <see cref="LanguageContentReference" /> is equal to another <see cref="LanguageContentReference" />.
        /// </summary>
        /// <param name="other">A <see cref="LanguageContentReference" /> to compare with this object.</param>
        /// <param name="ignoreVersion">Indicates if version information should be excluded from the comparison.</param>
        /// <returns>
        /// true if the current object is considered equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(LanguageContentReference other, bool ignoreVersion)
        {
            if (other is null) return false;

            return ContentLink.Equals(other.ContentLink, ignoreVersion) && Language.Equals(other.Language);
        }

        /// <inheritdoc />
        public bool Equals(LanguageContentReference other) => Equals(other, false);

        /// <inheritdoc />
        public override bool Equals(object obj) => Equals(obj as LanguageContentReference);
    }
}
