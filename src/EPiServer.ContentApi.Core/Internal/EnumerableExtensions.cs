using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.ContentApi.Core.Internal
{
    /// <summary>
    /// Internal extensions for <see cref="IEnumerable{T}"/>
    /// </summary>
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Converts an <see cref="IEnumerable{T}"/> type to <see cref="IList{T}" />.
        /// If already a list, it will be returned unchanged; otherwise the enumerable will be realized.
        /// </summary>
        /// <remarks>Note that the returned list can be an array and therefor read-only.</remarks>
        public static IList<T> AsList<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable is null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }

            return enumerable is IList<T> list ? list : enumerable.ToList();
        }
    }
}
