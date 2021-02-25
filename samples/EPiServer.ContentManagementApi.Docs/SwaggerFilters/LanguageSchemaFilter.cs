using System;
using System.Collections.Generic;
using EPiServer.ContentApi.Core.Serialization.Models;
using Swashbuckle.Swagger;

namespace EPiServer.ContentManagementApi.Docs.SwaggerFilters
{
    public class LanguageSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {

            if (type == typeof(LanguageModel))
            {
                var name = nameof(LanguageModel.Name).ToLower();
                var displayName = ContentManagementApiFilter.ToCamelCase(nameof(LanguageModel.DisplayName));

                schema.required = new List<string> { name };
                schema.properties[displayName].readOnly = true;
            }
        }
    }
}
