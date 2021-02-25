using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
#pragma warning disable CS0618 //Wraps obsolete
    internal class ContentModelWrapper : IContentConverterProvider, IContentConverter
    {
        private readonly IContentModelMapper _contentModelWrapper;


        public ContentModelWrapper(IContentModelMapper contentModelMapper)
        {
            _contentModelWrapper = contentModelMapper;
        }

        public int SortOrder => (_contentModelWrapper as ContentModelMapperBase)?.Order ?? int.MaxValue;

        public IContentConverter Resolve(IContent content)
        {
            if (_contentModelWrapper is ContentModelMapperBase mapperBase)
            {
                return mapperBase.CanHandle(content) ? this : null;
            }

            return this;
        }

        public ContentApiModel Convert(IContent content, ConverterContext contentMappingContext) =>
            _contentModelWrapper.TransformContent(content, contentMappingContext.ExcludePersonalizedContent, string.Join("," ,contentMappingContext.ExpandedProperties));
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
