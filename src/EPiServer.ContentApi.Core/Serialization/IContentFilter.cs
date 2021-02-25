using EPiServer.Core;
using System;

namespace EPiServer.ContentApi.Core.Serialization
{
    /// <summary>
    /// Signature for component that can filter a <see cref="IContent"/> instance or perform some operations before it gets converted.
    /// Implemenations should inherit <see cref="ContentFilter{T}"/> and registered as <see cref="IContentFilter"/> in IOC container
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    public interface IContentFilter
    {
        /// <summary>
        /// Implementation will receive a writable content instance where it can for example remove/null a property before it gets converted.
        /// </summary>
        /// <param name="content">The content item to be converted</param>
        /// <param name="converterContext">The current converter context</param>
        void Filter(IContent content, ConverterContext converterContext);

        /// <summary>
        /// The content model the implementation handles
        /// </summary>
        Type HandledContentModel { get; }
    }

    /// <summary>
    /// Base class that can be used for components that wants to change a <see cref="IContent"/> instance or perform some operations before it is converted
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    /// <typeparam name="T">The content model T that the implementation handles.</typeparam>
    public abstract class ContentFilter<T> : IContentFilter where T : IContent
    {
        /// <inherit-doc/>
        Type IContentFilter.HandledContentModel => typeof(T);

        /// <inherit-doc/>
        void IContentFilter.Filter(IContent content, ConverterContext converterContext) => Filter((T)content, converterContext);

        /// <summary>
        /// Implementation will receive a writable content instance where it can for example remove/null a property before it gets converted.
        /// </summary>
        /// <param name="content">The content item to be converted</param>
        /// <param name="converterContext">The current converter context</param>
        public abstract void Filter(T content, ConverterContext converterContext);
    }
}
