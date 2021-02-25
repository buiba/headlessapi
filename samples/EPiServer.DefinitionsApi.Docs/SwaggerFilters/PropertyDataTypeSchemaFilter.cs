using System;
using EPiServer.DefinitionsApi.PropertyDataTypes;
using Swashbuckle.Swagger;

namespace EPiServer.DefinitionsApi.Docs.SwaggerFilters
{
    // Using an crude hack here for PropertyDataTypes as a more generic solution
    // would have to be rewritten when moved to .NET Core as Schema filters has changed
    public class PropertyDataTypeSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (type == typeof(ExternalPropertyDataType))
            {
                schema.properties["dataType"].readOnly = null;
                schema.properties["itemType"].readOnly = null;
            }
        }
    }
}
