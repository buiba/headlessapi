using System;
using System.IO;
using System.Net.Http.Headers;
using System.Web.Http;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentManagementApi.Controllers;
using EPiServer.ContentManagementApi.Docs;
using EPiServer.ContentManagementApi.Docs.SwaggerFilters;
using Newtonsoft.Json.Serialization;
using Swashbuckle.Application;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace EPiServer.ContentManagementApi.Docs
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                    {
                        c.SingleApiVersion("2-18", "EPiServer Content Management API");

                        c.IgnoreObsoleteActions();
                        c.IgnoreObsoleteProperties();

                        c.SchemaId(ContentManagementApiFilter.SchemaIdStrategy);

                        c.DocumentFilter<ContentManagementApiFilter>();
                        c.SchemaFilter<ContentManagementApiFilter>();
                        c.OperationFilter<HeaderOperationFilter>();

                        c.DocumentFilter<RemovalModelFilter>();
                        c.OperationFilter<RemovalModelFilter>();

                        c.SchemaFilter<ContentSchemaFilter>();
                        c.SchemaFilter<ContentReferenceSchemaFilter>();
                        c.SchemaFilter<LanguageSchemaFilter>();
                        c.SchemaFilter<PatchContentModelSchemaFilter>();

                        c.IncludeXmlComments(GetXmlCommentsPath<ContentManagementApiController>());
                        c.IncludeXmlComments(GetXmlCommentsPath<ContentApiModel>());

                        c.DescribeAllEnumsAsStrings(true);
                    })
                .EnableSwaggerUi(c =>
                {
                    //disables the interactive mode
                    c.SupportedSubmitMethods();

                    c.DocumentTitle("Episerver Content Management API");
                });

            // Remove all formatters but JSON
            var formatters = GlobalConfiguration.Configuration.Formatters;
            var jsonFormatter = formatters.JsonFormatter;

            // CamelCase for all property names
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

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
