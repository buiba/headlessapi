using System;
using EPiServer.DefinitionsApi.PropertyGroups;
using Swashbuckle.Swagger;

namespace EPiServer.DefinitionsApi.Docs.SwaggerFilters
{
    // Using an crude hack here for PropertyGroup as a more generic solution
    // would have to be rewritten when moved to .NET Core as Schema filters has changed
    public class PropertyGroupSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (type == typeof(PropertyGroupModel))
            {
                schema.properties["systemGroup"].readOnly = true;
            }
        }
    }
}
