using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Core.ContentResult
{
    /// <summary>
    /// Component that resolves which <see cref="IContentApiSerializer"/> that should be used
    /// <para>
    /// Preview API: This API is current in preview state meaning it might change between minor versions
    /// </para>
    /// </summary>
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    public class ContentApiSerializerResolver
    {
        private readonly IJsonSerializerConfiguration _jsonSerializerConfiguration;
        private readonly IContentApiSerializer _defaultSerializer;
        private IContentApiSerializer _nullCleaningSerializer;

        //Exposed for unit tests
        protected ContentApiSerializerResolver()
        { }

        /// <summary>
        /// Creates a new instance of <see cref="ContentApiSerializerResolver"/>
        /// </summary>
        public ContentApiSerializerResolver(IJsonSerializerConfiguration jsonSerializerConfiguration, IContentApiSerializer defaultSerializer)
        {
            _jsonSerializerConfiguration = jsonSerializerConfiguration;
            _defaultSerializer = defaultSerializer;
        }

        /// <summary>
        /// Resolves which <see cref="IContentApiSerializer"/> given specified <paramref name="contentApiOptions"/>
        /// </summary>
        /// <param name="contentApiOptions">The options to resolve serializer for</param>
        /// <returns>A <see cref="IContentApiSerializer"/> instance</returns>
        public IContentApiSerializer Resolve(ContentApiOptions contentApiOptions)
        {
            if (contentApiOptions is null || contentApiOptions.IncludeNullValues)
            {
                return _defaultSerializer;
            }
            else
            {
                if (_nullCleaningSerializer is null)
                {
                    var copyOfSettings = CopyConfiguredSettings();
                    copyOfSettings.NullValueHandling = NullValueHandling.Ignore;
                    _nullCleaningSerializer = new NullCleaningContentSerializer(new Core.ContentResult.Internal.JsonSerializer { Settings = copyOfSettings });
                }
                return _nullCleaningSerializer;
            }
        }

        private JsonSerializerSettings CopyConfiguredSettings()
        {
            //unfortunately there is no Copy/Clone method on JsonSerializerSettings
            var configuredSettings = _jsonSerializerConfiguration.Settings;
            var copiedSerializer = new JsonSerializerSettings
            {
                Context = configuredSettings.Context,
                Culture = configuredSettings.Culture,
                ContractResolver = configuredSettings.ContractResolver,
                ConstructorHandling = configuredSettings.ConstructorHandling,
                CheckAdditionalContent = configuredSettings.CheckAdditionalContent,
                DateFormatHandling = configuredSettings.DateFormatHandling,
                DateFormatString = configuredSettings.DateFormatString,
                DateParseHandling = configuredSettings.DateParseHandling,
                DateTimeZoneHandling = configuredSettings.DateTimeZoneHandling,
                DefaultValueHandling = configuredSettings.DefaultValueHandling,
                EqualityComparer = configuredSettings.EqualityComparer,
                FloatFormatHandling = configuredSettings.FloatFormatHandling,
                Formatting = configuredSettings.Formatting,
                FloatParseHandling = configuredSettings.FloatParseHandling,
                MaxDepth = configuredSettings.MaxDepth,
                MetadataPropertyHandling = configuredSettings.MetadataPropertyHandling,
                MissingMemberHandling = configuredSettings.MissingMemberHandling,
                NullValueHandling = configuredSettings.NullValueHandling,
                ObjectCreationHandling = configuredSettings.ObjectCreationHandling,
                PreserveReferencesHandling = configuredSettings.PreserveReferencesHandling,
                ReferenceLoopHandling = configuredSettings.ReferenceLoopHandling,
                StringEscapeHandling = configuredSettings.StringEscapeHandling,
                TraceWriter = configuredSettings.TraceWriter,
                TypeNameHandling = configuredSettings.TypeNameHandling,
                SerializationBinder = configuredSettings.SerializationBinder,
                TypeNameAssemblyFormatHandling = configuredSettings.TypeNameAssemblyFormatHandling,
                ReferenceResolverProvider = configuredSettings.ReferenceResolverProvider
            };
            foreach (var converter in configuredSettings.Converters)
            {
                copiedSerializer.Converters.Add(converter);
            }
            return copiedSerializer;
        }
    }
}
