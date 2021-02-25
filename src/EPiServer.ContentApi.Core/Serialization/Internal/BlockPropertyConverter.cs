using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
    internal class BlockPropertyConverter : IPropertyConverter
    {
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly ReflectionService _reflectionService;
        private IPropertyConverterResolver _propertyConverterResolver;

        public BlockPropertyConverter(IContentTypeRepository contentTypeRepository, ReflectionService reflectionService, IPropertyConverterResolver propertyConverterResolver)
        {
            _contentTypeRepository = contentTypeRepository;
            _reflectionService = reflectionService;
            _propertyConverterResolver = propertyConverterResolver;
        }


        public IPropertyModel Convert(PropertyData propertyData, ConverterContext contentMappingContext)
        {
            return new BlockPropertyModel
            {
                ExcludePersonalizedContent = contentMappingContext.ExcludePersonalizedContent,
                Name = propertyData.Name,
                PropertyDataProperty = (PropertyBlock)propertyData,
                Properties = ExtractProperties((IPropertyBlock)propertyData, contentMappingContext)
            };
        }

        private IDictionary<string, object> ExtractProperties(IPropertyBlock propertyBlock, ConverterContext contentMappingContext)
        {
            var blockProperties = new Dictionary<string, object>();
            var contentType = _contentTypeRepository.Load(propertyBlock.BlockType);
            foreach (var property in propertyBlock.Property)
            {
                if (contentType is object && ShouldPropertyBeIgnored(contentType, property))
                {
                    continue;
                }

                var converter = _propertyConverterResolver.Resolve(property);
                if (converter != null)
                {
                    var propertyModel = converter.Convert(property, new ConverterContext(contentMappingContext));
                    blockProperties.Add(property.Name, propertyModel);
                }
               
            }

            return blockProperties;
        }

        private bool ShouldPropertyBeIgnored(ContentType contentType, PropertyData property)
        {
            var propertyAttributes = _reflectionService.GetAttributes(contentType, property);
            return propertyAttributes != null && propertyAttributes.OfType<JsonIgnoreAttribute>().Any();
        }

    }
}
