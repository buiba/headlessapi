using System;
using EPiServer.DefinitionsApi.ContentTypes.Manifest;
using EPiServer.DefinitionsApi.Manifest;
using EPiServer.DefinitionsApi.PropertyGroups.Manifest;
using Swashbuckle.Swagger;

namespace EPiServer.DefinitionsApi.Docs.SwaggerFilters
{
    public class ManifestSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (type == typeof(ManifestModel))
            {
                // Define known import sets here
                schema.properties["contentTypes"] = schemaRegistry.GetOrRegister(typeof(ContentTypesSection));
                schema.properties["propertyGroups"] = schemaRegistry.GetOrRegister(typeof(PropertyGroupsSection));
            }
        }
    }
}
