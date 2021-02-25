using System;
using System.Collections.Generic;
using EPiServer.ContentApi.Core.Serialization.Models;
using Swashbuckle.Swagger;

namespace EPiServer.ContentManagementApi.Docs.SwaggerFilters
{
    public class ContentReferenceSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (type == typeof(ContentModelReference))
            {
                schema.properties?.Remove(nameof(ContentModelReference.Expanded).ToLower());

                var languageKey = nameof(ContentModelReference.Language).ToLower();
                schema.properties[nameof(ContentModelReference.Url).ToLower()].readOnly = true;
                schema.properties[languageKey].readOnly = true;

                // Readonly for property is object
                schema.properties[languageKey].allOf = new List<Schema> { new Schema { @ref = schema.properties[languageKey].@ref } };
                schema.properties[languageKey].@ref = null;
            }
        }
    }
}
