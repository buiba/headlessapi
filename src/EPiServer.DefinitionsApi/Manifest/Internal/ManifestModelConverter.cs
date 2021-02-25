using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EPiServer.DefinitionsApi.Manifest.Internal
{
    internal class ManifestModelConverter : JsonConverter
    {
        private static readonly ILogger Log = LogManager.GetLogger(typeof(ManifestModelConverter));

        private readonly ManifestSectionImporterResolver _manifestSectionImporterResolver;

        public ManifestModelConverter()
            : this(ServiceLocator.Current.GetInstance<ManifestSectionImporterResolver>())
        { }

        internal ManifestModelConverter(ManifestSectionImporterResolver manifestSectionImporterResolver)
        {
            // Avoid failing if the JsonConverter is used outside of a service scope (such as OpenAPI generation)
            _manifestSectionImporterResolver = manifestSectionImporterResolver ?? new ManifestSectionImporterResolver();
        }

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => typeof(ManifestModel).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var instance = (ManifestModel)Activator.CreateInstance(objectType);

            var instanceProperties = objectType
                .GetProperties()
                .ToList();

            foreach (var property in JObject.Load(reader).Properties())
            {
                if (property.Value.Type == JTokenType.Object)
                {
                    var sectionType = _manifestSectionImporterResolver.ResolveManifestSectionType(property.Name);

                    if (sectionType is object)
                    {
                        var section = property.Value.ToObject(sectionType, serializer) as IManifestSection;

                        if (section is object)
                        {
                            instance.Sections.Add(property.Name, section);
                        }
                    }
                    else
                    {
                        if (!SetPropertyValue(instance, instanceProperties, property, serializer))
                        {
                            Log.Warning($"There is no manifest section importer (IManifestSectionImporter) registered for '{property.Name}'.");
                        }
                    }
                }
                else
                {
                    SetPropertyValue(instance, instanceProperties, property, serializer);
                }
            }

            return instance;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private bool SetPropertyValue(ManifestModel instance, List<PropertyInfo> instanceProperties, JProperty property, JsonSerializer serializer)
        {
            var instanceProperty = instanceProperties.Find(x =>
                x.Name.Equals(property.Name, StringComparison.OrdinalIgnoreCase) &&
                x.CanWrite);

            if (instanceProperty is object)
            {
                try
                {
                    instanceProperty.SetValue(instance, property.Value.ToObject(instanceProperty.PropertyType, serializer));
                }
                catch (Exception ex)
                {
                    if (serializer.MissingMemberHandling == MissingMemberHandling.Error)
                    {
                        Log.Error("Unable to set instance property value.", ex);

                        throw;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else if (serializer.MissingMemberHandling == MissingMemberHandling.Error)
            {
                throw new MissingMemberException(nameof(ManifestModel), property.Name);
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
