﻿using System;
using System.Linq;
using System.Reflection;
using System.Web.Http.Description;
using EPiServer.ContentApi.Core.Internal;
using Swashbuckle.Swagger;

namespace EPiServer.ContentManagementApi.Docs.SwaggerFilters
{
    public class ContentManagementApiFilter : IDocumentFilter, ISchemaFilter
    {
        private const string SchemaRemovalExtension = "epi-remove";
        private const string ApiRoutePrefix = "/api/episerver/";

        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            var keys = swaggerDoc.paths.Keys.Where(x => !x.StartsWith(ApiRoutePrefix, StringComparison.OrdinalIgnoreCase));

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
            foreach (var path in swaggerDoc.paths.Values)
            {
                path.options = null;
            }
        }

        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (!type.Namespace.StartsWith("EPiServer.ContentManagementApi") && !type.Namespace.StartsWith("EPiServer.ContentApi.Core"))
            {
                schema.vendorExtensions[SchemaRemovalExtension] = true;
            }
        }

        public static string SchemaIdStrategy(Type type)
        {
            return GetDocName(type);
        }

        public static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
            {
                return s;
            }

            var chars = s.ToCharArray();
            chars[0] = char.ToLowerInvariant(chars[0]);
            return new string(chars);
        }

        public static string GetDocName(Type type)
        {
            var attr = type.GetCustomAttribute<ApiDefinitionAttribute>(false);
            return attr?.Name ?? ToCamelCase(type.Name);
        }
    }
}
