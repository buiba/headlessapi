using EPiServer.ContentApi.Core.ContentResult;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Framework.Serialization.Json.Internal;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.ContentApi.Search.Internal
{
    [ServiceConfiguration(typeof(IContentApiModelFilter), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class SearchContentModelFilter : ContentApiModelFilter<ContentApiModel>
    {
        private readonly IJsonSerializerConfiguration _jsonSerializerConfiguration;

        public SearchContentModelFilter(IJsonSerializerConfiguration jsonSerializerConfiguration)
        {
            _jsonSerializerConfiguration = jsonSerializerConfiguration;
        }

        public override void Filter(ContentApiModel contentApiModel, ConverterContext converterContext)
        {
            if (!(converterContext is FindIndexingJobConverterContext))
            {
                return;
            }

            // Currently, when we add an object to IDictionary and pass to Find for indexing,
            // Find will automatically add extra suffiex (Eg $$string, $$number, ..) to property name and it will make OData's
            // filter by property name does not work properly. To workaround, we should convert object to JToken before add to dictionary.
            contentApiModel.Properties = contentApiModel.Properties
                .Select(x =>
                {
                    var jTokenValue = x.Value == null ? null : JToken.FromObject(x.Value, JsonSerializer.Create(_jsonSerializerConfiguration.Settings));

                    return new KeyValuePair<string, object>(StringUtility.ToCamelCase(x.Key), jTokenValue);
                })
                .ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
