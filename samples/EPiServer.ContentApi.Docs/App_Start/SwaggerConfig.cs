using System.Web.Http;
using WebActivatorEx;
using EPiServer.ContentApi.Docs;
using Swashbuckle.Application;
using System;
using System.IO;
using System.Linq;
using EPiServer.ContentApi.Cms.Controllers;
using EPiServer.ContentApi.Search.Controllers;
using Swashbuckle.Swagger;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace EPiServer.ContentApi.Docs
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration

                .EnableSwagger(c =>
                    {

                        c.SingleApiVersion("2-18", "EPiServer.ContentDeliveryApi");
                        //c.SingleApiVersion("2.8.0", "EPiServer.ContentDelivery"); "." breaks the swagger output!?!

                        c.IgnoreObsoleteActions();
                        c.IgnoreObsoleteProperties();

                        c.DocumentFilter<ContentDeliveryApiFilter>();
                        c.DocumentFilter<OAuthEndpointDocFilter>();

                        c.SchemaFilter<ContentDeliveryApiFilter>();

                        c.OperationFilter<ContentApiOperationFiler>();

                        c.IncludeXmlComments(GetXmlCommentsPath<ContentApiController>());
                        c.IncludeXmlComments(GetXmlCommentsPath<ContentApiSearchController>());
                        c.IncludeXmlComments(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "EPiServer.ContentApi.Commerce.xml"));

                        //todo: workaround that picks the first action in case of conflicts. Conflicts currently exist in cms api and search api.
                        c.ResolveConflictingActions( x => x.First());

                        c.MapType<decimal>(() => new Schema { type = "number", format = "decimal" });
                        c.MapType<decimal?>(() => new Schema { type = "number", format = "decimal" });

                        c.DescribeAllEnumsAsStrings();
                    })

                .EnableSwaggerUi(c => {

                    //disables the interactive mode
                    c.SupportedSubmitMethods();

                    c.DocumentTitle("Episerver Content Delivery API");
                });
        }

        private static string GetXmlCommentsPath<TAssemblyType>()
        {
            var commentsFileName = typeof(TAssemblyType).Assembly.GetName().Name + ".xml";
            return Path.Combine(AppDomain.CurrentDomain.RelativeSearchPath, commentsFileName);
        }
    }
}
