using EPiServer.ContentApi.Core.Serialization.Models;
using System;

namespace EPiServer.ContentApi.Core.Serialization
{
    /// <summary>
    /// Signature for component that can filter a <see cref="ContentApiModel"/> instance before it gets serialized.
    /// Implemenations should inherit <see cref="ContentApiModelFilter{T}"/> and registered as <see cref="IContentApiModelFilter"/> in IOC container
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    public interface IContentApiModelFilter
    {
        /// <summary>
        /// Implementations will be called with the created <paramref name="contentApiModel"/> before it gets serialized
        /// </summary>
        /// <param name="contentApiModel">The content api model to be serialized</param>
        /// <param name="converterContext">The current converter context</param>
        void Filter(ContentApiModel contentApiModel, ConverterContext converterContext);

        /// <summary>
        /// The content api model the implementation handles
        /// </summary>
        Type HandledContentApiModel { get; }
    }

    /// <summary>
    /// Base class that can be used for components that wants to change a <see cref="ContentApiModel"/> instance before it is serialized
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    /// <typeparam name="T">The content model T that the implementation handles.</typeparam>
    public abstract class ContentApiModelFilter<T> : IContentApiModelFilter where T : ContentApiModel
    {
        /// <inherit-doc/>
        Type IContentApiModelFilter.HandledContentApiModel => typeof(T);

        /// <inherit-doc/>
        void IContentApiModelFilter.Filter(ContentApiModel contentApiModel, ConverterContext converterContext) => Filter((T)contentApiModel, converterContext);

        /// <summary>
        /// Implementations will be called with the created <paramref name="contentApiModel"/> before it gets serialized
        /// </summary>
        /// <param name="contentApiModel">The content api model to be serialized</param>
        /// <param name="converterContext">The current converter context</param>
        public abstract void Filter(T contentApiModel, ConverterContext converterContext);
    }
}
