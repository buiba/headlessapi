using EPiServer.Core;
using EPiServer.ServiceLocation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
    // plan is to obsolete below with [Obsolete("Has been replaced by IContentConverterResolver")]

    /// <summary>
    /// Factory to decide what mapper should be used to transform content
    /// </summary>
    public interface IContentModelMapperFactory
    {
        /// <summary>
        /// Get mapper for a specific content
        /// </summary>
        IContentModelMapper GetMapper<T>(T content) where T: class;
    }

    // plan is to obsolete below with [Obsolete("Has been replaced by IContentConverterResolver")]

    /// <summary>
    /// Default implementation of IContentModelMapperFactory
    /// </summary>
    [ServiceConfiguration(typeof(IContentModelMapperFactory))]
    public class ContentModelMapperFactory : IContentModelMapperFactory
    {
        protected IEnumerable<IContentModelMapper> _mappers;
        public ContentModelMapperFactory(IEnumerable<IContentModelMapper> mappers)
        {
            _mappers = mappers;
        }

        /// <inheritdoc />
        public virtual IContentModelMapper GetMapper<T>(T content) where T: class
        {
            // support old mapper that inherits only IContentModelMapper
            var oldContentMapperList = _mappers.Where(item => !(item is ContentModelMapperBase));
            if (oldContentMapperList != null && oldContentMapperList.Any())
            {
                return oldContentMapperList.FirstOrDefault();
            }

            // support new mapper that inherits ContentModelMapperBase
            var contentModelMapperBaseList = _mappers.Select(mapper => { return mapper as ContentModelMapperBase; })
                                                        .Where(item => item != null && item.CanHandle(content))
                                                        .OrderByDescending(mapper => mapper.Order);
            return contentModelMapperBaseList.FirstOrDefault();
        }        
    }
}
