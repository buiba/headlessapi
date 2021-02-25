using System;
using System.IO;
using System.Net.Http.Headers;
using System.Web.Http;
using EPiServer.DefinitionsApi.ContentTypes;
using EPiServer.DefinitionsApi.ContentTypes.Internal;
using EPiServer.DefinitionsApi.Docs;
using EPiServer.DefinitionsApi.Docs.SwaggerFilters;
using Swashbuckle.Application;
using Swashbuckle.Swagger;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace EPiServer.DefinitionsApi.Docs
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                    {
                        c.SingleApiVersion("2-18", "EPiServer Definitions API");

                        c.IgnoreObsoleteActions();
                        c.IgnoreObsoleteProperties();

                        c.SchemaId(DefinitionsApiFilter.SchemaIdStrategy);

                        c.DocumentFilter<DefinitionsApiFilter>();
                        c.SchemaFilter<DefinitionsApiFilter>();

                        c.SchemaFilter<ManifestSchemaFilter>();
                        c.SchemaFilter<PropertyDataTypeSchemaFilter>();
                        c.SchemaFilter<PropertyGroupSchemaFilter>();

                        c.IncludeXmlComments(GetXmlCommentsPath<ContentTypesController>());

                        c.DescribeAllEnumsAsStrings(true);

                        // Map ValidatableExternalContentType to standard ExternalContentType
                        c.MapType<ValidatableExternalContentType>(() => new Schema { @ref = $"#/definitions/{ExternalContentType.DefinitionName}" });
                    })
                .EnableSwaggerUi(c =>
                {
                    //disables the interactive mode
                    c.SupportedSubmitMethods();

                    c.DocumentTitle("Episerver Definitions API");
                });

            // Remove all formatters but JSON
            var formatters = GlobalConfiguration.Configuration.Formatters;
            var jsonFormatter = formatters.JsonFormatter;
            formatters.Clear();
            formatters.Add(jsonFormatter);

            // Only support "application/json"
            jsonFormatter.SupportedMediaTypes.Clear();
            jsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
        }

        private static string GetXmlCommentsPath<TAssemblyType>()
        {
            var commentsFileName = typeof(TAssemblyType).Assembly.GetName().Name + ".xml";
            return Path.Combine(AppDomain.CurrentDomain.RelativeSearchPath, commentsFileName);
        }
    }
}
