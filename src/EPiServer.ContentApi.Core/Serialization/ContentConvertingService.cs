using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.Data.Entity;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.Serialization
{
    /// <summary>
    /// Handles conversions from <see cref="IContent"/> instances to <see cref="ContentApiModel"/> instances.
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    /// <remarks>
    /// Custom content converters are registered by registering <see cref="IContentConverterProvider"/> implementations in IOC container.
    /// Custom property converters are registered by registring <see cref="IPropertyConverterProvider"/> implementation in IOC container.
    /// </remarks>
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    public class ContentConvertingService
    {
        private readonly IContentConverterResolver _contentConverterResolver;
        private readonly IEnumerable<IContentFilter> _contentFilters;
        private readonly IEnumerable<IContentApiModelFilter> _contentApiModelFilters;
        private readonly ContentApiModelMinifier _contentApiModelMinifier;
        private readonly ConcurrentDictionary<Type, List<IContentFilter>> _contentFiltersPerType = new ConcurrentDictionary<Type, List<IContentFilter>>();
        private readonly ConcurrentDictionary<Type, List<IContentApiModelFilter>> _contentModelFiltersPerType = new ConcurrentDictionary<Type, List<IContentApiModelFilter>>();

        /// <summary>
        /// Only to be used from unit tests
        /// </summary>
        public ContentConvertingService()
        { }

        /// <summary>
        /// Creates a new instance of <see cref="ContentConvertingService"/>
        /// </summary>
        /// <param name="contentConverterResolver"></param>
        /// <param name="contentFilters"></param>
        /// <param name="contentApiModelFilters"></param>
        public ContentConvertingService(IContentConverterResolver contentConverterResolver, IEnumerable<IContentFilter> contentFilters, IEnumerable<IContentApiModelFilter> contentApiModelFilters)
        {
            _contentConverterResolver = contentConverterResolver;
            _contentFilters = contentFilters;
            _contentApiModelFilters = contentApiModelFilters;
            _contentApiModelMinifier = new ContentApiModelMinifier();
        }

        /// <summary>
        /// Converts a <see cref="IContent"/> instance to a <see cref="ContentApiModel"/> model to use from an expanded property
        /// </summary>
        /// <param name="content">The content instance to convert</param>
        /// <param name="converterContext">The current conversion context</param>
        /// <returns>A created <see cref="ContentApiModel"/></returns>
        public virtual ContentApiModel ConvertToContentApiModel(IContent content, ConverterContext converterContext)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            //Apply content filters before creating model
            var contentFilters = GetContentFilters(content.GetType());
            if (contentFilters.Any())
            {
                //We create a writable model so filters can modify properties
                content = content is IReadOnly readOnly ? (IContent)readOnly.CreateWritableClone() : content;
                contentFilters.ForEach(c => c.Filter(content, converterContext));
            }

            //Create the content api model
            var contentApiModel = _contentConverterResolver.Resolve(content).Convert(content, converterContext);

            //Apply content model filters
            GetContentModelFilters(contentApiModel).ForEach(c => c.Filter(contentApiModel, converterContext));

            return contentApiModel;
        }

        /// <summary>
        /// Converts a <see cref="IContent"/> instance to a <see cref="IContentApiModel"/>
        /// </summary>
        /// <param name="content">The content instance to convert</param>
        /// <param name="converterContext">The current conversion context</param>
        /// <returns>A created <see cref="IContentApiModel"/></returns>
        public virtual IContentApiModel Convert(IContent content, ConverterContext converterContext)
        {
            var defaultModel = ConvertToContentApiModel(content, converterContext);
            return converterContext.SelectedProperties.Any() ? _contentApiModelMinifier.Minify(defaultModel, converterContext.SelectedProperties) : (IContentApiModel)defaultModel;
        }

        private List<IContentFilter> GetContentFilters(Type contentModel) =>
            _contentFiltersPerType.GetOrAdd(contentModel, t => _contentFilters.Where(f => f.HandledContentModel.IsAssignableFrom(t)).ToList());

        private List<IContentApiModelFilter> GetContentModelFilters(ContentApiModel contentApiModel) =>
            _contentModelFiltersPerType.GetOrAdd(contentApiModel.GetType(), t => _contentApiModelFilters.Where(f => f.HandledContentApiModel.IsAssignableFrom(t)).ToList());
    }
}
