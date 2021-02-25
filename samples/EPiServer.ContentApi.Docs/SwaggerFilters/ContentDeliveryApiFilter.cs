using System;
using System.Linq;
using System.Web.Http.Description;
using EPiServer.ContentApi.Core;
using Swashbuckle.Swagger;

namespace EPiServer.ContentApi.Docs
{
    public class ContentDeliveryApiFilter : IDocumentFilter, ISchemaFilter
    {
        private const string SchemaRemovalExtension = "epi-remove";

        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            var apiPathVersionTwo = "/" + RouteConstants.VersionTwoApiRoute;
            var apiPathVersionThree = "/" + RouteConstants.VersionThreeApiRoute;

            var keys = swaggerDoc.paths.Keys.Where(x => !x.StartsWith(apiPathVersionTwo, StringComparison.OrdinalIgnoreCase)
                                                       && !x.StartsWith(apiPathVersionThree, StringComparison.OrdinalIgnoreCase)).ToArray();

            foreach (var key in keys)
            {
                swaggerDoc.paths.Remove(key);
            }

            var invalidSchemas = schemaRegistry.Definitions.Where(x => x.Value.vendorExtensions.ContainsKey(SchemaRemovalExtension)).Select(x => x.Key).ToArray();

            foreach (var key in invalidSchemas)
            {
                schemaRegistry.Definitions.Remove(key);
            }

            // remove OPTIONS METHOD
            foreach (PathItem path in swaggerDoc.paths.Values)
            {
                path.options = null;
            }
        }

        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (!type.Namespace.StartsWith("EPiServer.ContentApi"))
            {
                schema.vendorExtensions[SchemaRemovalExtension] = true;
            }
        }
    }
}
