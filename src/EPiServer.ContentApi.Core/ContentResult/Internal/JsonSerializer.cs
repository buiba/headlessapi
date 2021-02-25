using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace EPiServer.ContentApi.Core.ContentResult.Internal
{
    /// <summary>
    /// Serialize object into json
    /// </summary>
    public class JsonSerializer : IContentApiSerializer, IJsonSerializerConfiguration
    {

        private JsonSerializerSettings _settings;

        /// <summary>
        /// Media type used when build response. Ex: application/json, application/xml
        /// </summary>
        public string MediaType { get { return "application/json"; } }

        /// <summary>
        /// Encoding used when build response
        /// </summary>
        public Encoding Encoding { get { return Encoding.UTF8; } }

        /// <summary>
        /// serialize object to JSON string using Settings as serializer settings
        /// </summary>
        public virtual string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, Settings);
        }

        /// <summary>
        /// Default JSON serializer settings. This settings will be used in Serialize()
        /// </summary>
        public virtual JsonSerializerSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new JsonSerializerSettings
                    {
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                        ContractResolver = new DefaultContractResolver
                        {
                            NamingStrategy = new CamelCaseNamingStrategy
                            {
                                ProcessDictionaryKeys = true,
                                ProcessExtensionDataNames = true,
                                OverrideSpecifiedNames = true
                            }
                        }
                    };
                }

                return _settings;
            }
            set
            {
                _settings = value;
            }
        }
    }
}
