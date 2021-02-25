using System;
using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.Internal;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    /// <summary>
    /// Represents a token used to keep track of the position in a paged result.
    /// </summary>
    internal readonly struct ContinuationToken : IEquatable<ContinuationToken>
    {
        /// <summary>
        /// Standard name of the ContinuationToken header.
        /// </summary>
        internal const string HeaderName = "x-epi-continuation";

        private static readonly char[] TokenStringValueDividers = new[] { '|' };

        /// <summary>
        /// An empty token.
        /// </summary>
        public static readonly ContinuationToken None = default;

        /// <summary>
        /// Initialize a new instances of the <see cref="ContinuationToken"/> struct.
        /// </summary>
        /// <param name="skip">The place where the next set should start.</param>
        /// <param name="take">The page size.</param>
        public ContinuationToken(int skip, int take)
        {
            Take = take;
            Skip = skip;
        }

        /// <summary>
        /// The place where the next set should start.
        /// </summary>
        public readonly int Skip;

        /// <summary>
        /// The page size.
        /// </summary>
        public readonly int Take;

        /// <inheritdoc />
        public static bool operator ==(ContinuationToken left, ContinuationToken right) => left.Equals(right);

        /// <inheritdoc />
        public static bool operator !=(ContinuationToken left, ContinuationToken right) => !left.Equals(right);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => obj is ContinuationToken other && Equals(other);

        /// <inheritdoc/>
        public bool Equals(ContinuationToken other) => Take == other.Take && Skip == other.Skip;

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            var hash = HashCodeCombiner.Start();
            hash.Add(Take);
            hash.Add(Skip);
            return hash.CombinedHash;
        }

        /// <inheritdoc />
        public override string ToString() => string.Concat(Skip, TokenStringValueDividers[0], Take);

        /// <summary>
        /// Converts the token to a Base64 encoded string. This string is to be considered as an opaque string to external users.
        /// </summary>
        /// <returns>A Base64 encoded token string.</returns>
        public string AsTokenString() => Convert.ToBase64String(Encoding.UTF8.GetBytes(ToString()));

        /// <summary>
        /// Parses a Base64 encoded token string to a new <see cref="ContinuationToken"/> instance.
        /// </summary>
        /// <param name="tokenString">The string to parse.</param>
        /// <param name="token">The resulting token.</param>
        /// <returns>A new <see cref="ContinuationToken"/> instance. If the string is null or empty, <see cref="None"/> is returned. </returns>
        public static bool TryParseTokenString(string tokenString, out ContinuationToken token)
        {
            if (string.IsNullOrWhiteSpace(tokenString))
            {
                token = default;
                return true;
            }

            string str;
            try
            {
                str = Encoding.UTF8.GetString(Convert.FromBase64String(tokenString));
            }
            catch (FormatException)
            {
                token = default;
                return false;
            }

            var parts = str.Split(TokenStringValueDividers, 2);

            if (parts.Length == 2 && int.TryParse(parts[0], out var skip) && skip > 0 && int.TryParse(parts[1], out var take) && take > 0)
            {
                token = new ContinuationToken(skip, take);
                return true;
            }

            token = default;
            return false;
        }
    }
}
