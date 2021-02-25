using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentManagementApi.Models.Internal;
using Swashbuckle.Swagger;

namespace EPiServer.ContentManagementApi.Docs.SwaggerFilters
{
    public class RemovalModelFilter : IDocumentFilter, IOperationFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            // Remove definitions
            var redundantDefinitions = new List<string>() {
                ContentManagementApiFilter.GetDocName(typeof(ContentApiCreateModel)),
                ContentManagementApiFilter.GetDocName(typeof(ContentReferenceInputModel))
            };
            var removingKeys = schemaRegistry.Definitions.Where(definition => redundantDefinitions.Any(name => definition.Key.Equals(name, StringComparison.OrdinalIgnoreCase)))
                                                         .Select(x => x.Key).ToArray();
            foreach (var key in removingKeys)
            {
                schemaRegistry.Definitions.Remove(key);
            }

            // Change reference
            foreach (var definition in schemaRegistry.Definitions)
            {
                foreach (var prop in definition.Value?.properties)
                {
                    ChangeReference(prop.Value);
                }
            }
        }

        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            foreach (var param in operation.parameters)
            {
                ChangeReference(param.schema);
            }
        }

        private void ChangeReference(Schema schema)
        {
            if (schema == null || string.IsNullOrEmpty(schema.@ref))
            {
                return;
            }

            const string definitionPrefix = "#/definitions/{0}";
            if (schema.@ref == string.Format(definitionPrefix, ContentManagementApiFilter.GetDocName(typeof(ContentApiCreateModel))))
            {
                schema.@ref = string.Format(definitionPrefix, ContentManagementApiFilter.GetDocName(typeof(ContentApiModel)));
            }

            if (schema.@ref == string.Format(definitionPrefix, ContentManagementApiFilter.GetDocName(typeof(ContentReferenceInputModel))))
            {
                schema.@ref = string.Format(definitionPrefix, ContentManagementApiFilter.GetDocName(typeof(ContentModelReference)));
            }

        }
    }
}
