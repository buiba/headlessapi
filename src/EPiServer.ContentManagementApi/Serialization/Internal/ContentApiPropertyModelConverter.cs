using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    [ServiceConfiguration(typeof(ContentApiPropertyModelConverter), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class ContentApiPropertyModelConverter
    {
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly TypeModelResolver _typeModelResolver;
        private readonly IContentRepository _contentRepository;
        private readonly ConcurrentDictionary<string, TypeModel> _typeModelCache = new ConcurrentDictionary<string, TypeModel>();

        public ContentApiPropertyModelConverter(IContentTypeRepository contentTypeRepository,
            TypeModelResolver typeModelResolver,
            IContentRepository contentRepository)
        {
            _contentTypeRepository = contentTypeRepository;
            _typeModelResolver = typeModelResolver;
            _contentRepository = contentRepository;
        }

        public IDictionary<string, object> ConvertRawPropertiesToPropertyModels(string id,
            IDictionary<string, object> sources, JsonSerializer serializer)
        {
            if (!TryGetContent(id, out var content))
            {
                throw new ErrorException(HttpStatusCode.NotFound, $"The content with id ({id}) does not exist.");
            }

            var contentType = _contentTypeRepository.Load(content.ContentTypeID);
            return ConvertRawPropertiesToPropertyModels(contentType, sources, serializer);
        }

        public IDictionary<string, object> ConvertRawPropertiesToPropertyModels(ContentType contentType,
            IDictionary<string, object> sources, JsonSerializer serializer)
        {
            var propertyDefinitions = contentType.PropertyDefinitions;
            var propertyModels = new Dictionary<string, object>();
            foreach (var item in sources)
            {
                if (item.Key.Equals(nameof(ICategorizable.Category), StringComparison.OrdinalIgnoreCase))
                {
                    propertyModels.Add(nameof(ICategorizable.Category),
                        (item.Value as JObject)?.ToObject<CategoryPropertyModel>(serializer));
                    continue;
                }

                var propertyDefinition =
                    propertyDefinitions.FirstOrDefault(pd =>
                        pd.Name.Equals(item.Key, StringComparison.OrdinalIgnoreCase));
                if (propertyDefinition is null)
                {
                    throw new ErrorException(HttpStatusCode.BadRequest, $"Property definition for '{item.Key}' does not exist.");
                }

                var propertyModel = item.Value is JObject jObject ? ConvertRawDataToPropertyModel(jObject, propertyDefinition, serializer) : null;

                if (propertyModel is BlockPropertyModel blockPropertyModel && blockPropertyModel.Properties is object)
                {
                    // PropertyDataType is a built-in property of BlockPropertyModel, but it is a get-only property so
                    // when deserializing it is treated as extension data and is put into BlockPropertyModel.Properties.
                    // Therefore we need to remove that property from the dictionary.
                    var propertyDataTypeProp = blockPropertyModel.Properties.FirstOrDefault(x => x.Key.Equals(nameof(BlockPropertyModel.PropertyDataType), StringComparison.OrdinalIgnoreCase));
                    if (propertyDataTypeProp.Key is object)
                    {
                        blockPropertyModel.Properties.Remove(propertyDataTypeProp.Key);
                    }

                    var blockContentType = _contentTypeRepository.Load(propertyDefinition.Type.Name);
                    blockPropertyModel.Properties =
                        ConvertRawPropertiesToPropertyModels(blockContentType, blockPropertyModel.Properties, serializer);
                    propertyModels.Add(propertyDefinition.Name, blockPropertyModel);
                }
                else
                {
                    propertyModels.Add(propertyDefinition.Name, propertyModel);
                }
            }

            return propertyModels;
        }

        public IDictionary<string, object> ConvertRawPropertiesToPropertyModels(IEnumerable<string> contentTypes, IDictionary<string, object> sources, JsonSerializer serializer)
        {
            if (contentTypes is null || !contentTypes.Any())
            {
                throw new ErrorException(HttpStatusCode.BadRequest, "Content type is required.");
            }

            var modelContentType = contentTypes.Last();
            var contentType = _contentTypeRepository.Load(modelContentType);
            if (contentType is null)
            {
                throw new ErrorException(HttpStatusCode.BadRequest, $"Content type '{modelContentType}' does not exist.");
            }

            return ConvertRawPropertiesToPropertyModels(contentType, sources, serializer);
        }

        private bool TryGetContent(string id, out IContent content)
        {
            if (ContentReference.TryParse(id, out var contentReference))
            {
                return _contentRepository.TryGet(contentReference, out content);
            }

            if (Guid.TryParse(id, out var contentGuid))
            {
                return _contentRepository.TryGet(contentGuid, out content);
            }

            content = null;
            return false;

        }

        private object ConvertRawDataToPropertyModel(JToken propertyValue, PropertyDefinition propDefinition, JsonSerializer serializer)
        {
            var propType = (propDefinition.Type is BlockPropertyDefinitionType) ? typeof(PropertyBlock) : propDefinition.Type.DefinitionType;
            var modelType = _typeModelCache.GetOrAdd(propType.FullName, _typeModelResolver.Resolve(propType));
            if (modelType is null)
            {
                throw new ErrorException(HttpStatusCode.BadRequest, $"Cannot resolve IPropertyModel for '{propType.FullName}'.");
            }

            try
            {
                return propertyValue.ToObject(modelType.ModelType, serializer);
            }
            catch (Exception ex)
            {
                throw new ErrorException(HttpStatusCode.BadRequest, ex.Message);
            }
        }
    }
}
