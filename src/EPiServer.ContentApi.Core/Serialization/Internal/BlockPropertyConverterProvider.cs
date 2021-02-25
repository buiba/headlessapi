using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.SpecializedProperties;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
    internal class BlockPropertyConverterProvider : IPropertyConverterProvider
    {
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly ReflectionService _reflectionService;
        private IPropertyConverterResolver _propertyConverterResolver;
        private BlockPropertyConverter _blockPropertyModelConverter;

        public BlockPropertyConverterProvider(IContentTypeRepository contentTypeRepository, ReflectionService reflectionService)
        {
            _contentTypeRepository = contentTypeRepository;
            _reflectionService = reflectionService;
        }

        /// <summary>
        /// We handle block through an own IPropertyConverter rather than handling blocks through DefaultPropertyConverter so we can have a SortOrder so
        /// any existing partner implementations are favoured before default one.
        /// </summary>
        public int SortOrder => int.MinValue + 1000;

        //Since this class it self is a IPropertyConverterProvider we cant use constructor injection
        public void Initialize(IPropertyConverterResolver propertyConverterResolver) => _propertyConverterResolver = propertyConverterResolver;

        public IPropertyConverter Resolve(PropertyData propertyData)
        {
            if (propertyData is IPropertyBlock blockProperty)
            {
                if (!(_blockPropertyModelConverter is object))
                {
                    _blockPropertyModelConverter = new BlockPropertyConverter(_contentTypeRepository, _reflectionService, _propertyConverterResolver);
                }
                return _blockPropertyModelConverter;
            }

            return null;
        }
    }
}
