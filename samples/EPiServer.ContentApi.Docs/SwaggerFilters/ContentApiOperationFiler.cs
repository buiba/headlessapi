using EPiServer.ContentApi.Core;
using Swashbuckle.Swagger;
using System.Collections.Generic;
using System.Web.Http.Description;

namespace EPiServer.ContentApi.Docs
{
    public class ContentApiOperationFiler : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.parameters == null)
                operation.parameters = new List<Parameter>();
            var cloneParams = new List<Parameter>(operation.parameters);

            var controllerType = apiDescription.ActionDescriptor.ControllerDescriptor.ControllerType;

            // add Accept-Language and Authorization header to ContentDeliveryApi.Cms
            if (controllerType.Namespace.StartsWith("EPiServer.ContentApi.Cms.Controllers"))
            {
                operation.parameters.Add(new Parameter
                {
                    name = "Accept-Language",
                    @in = "header",
                    type = "string",
                    description = "Determines in which language the content should be retrieved. Example: 'en' or 'sv'",
                    @default = "",
                    required = false
                });

                operation.parameters.Add(new Parameter
                {
                    name = "Authorization",
                    @in = "header",
                    type = "string",
                    description = "It should be in the format 'Bearer your_token', where 'your_token' is the token provided after authorization to access API. "
                                  + "Note that there should be a space between 'Bearer' and 'your_token'.",
                    @default = "",
                    required = true
                });

                // remove languages parameter in query string
                foreach (var param in cloneParams)
                {
                    var isLanguageHeader = param.@in == "query" && param.name == "languages";
                    if (isLanguageHeader)
                    {
                        operation.parameters.Remove(param);
                    }
                }
            }

            // ContentDelivery Search: remove `request` param in body
            if (apiDescription.RelativePath.Contains(RouteConstants.VersionTwoApiRoute + "search/content"))
            {
                foreach (var param in cloneParams)
                {
                    var isRequestParam = param.@in == "query" && param.name.StartsWith("request");
                    if (isRequestParam)
                    {
                        operation.parameters.Remove(param);
                    }
                }

                // ------ we have to add parameters in query string manually ------- //

                operation.parameters.Add(new Parameter
                {
                    name = "filter",
                    @in = "query",
                    type = "string",
                    description = "Specify a filter string to be applied to the search. Filter strings utilize OData v4 filter syntax",
                    @default = "",
                    required = false
                });

                operation.parameters.Add(new Parameter
                {
                    name = "orderby",
                    @in = "query",
                    type = "string",
                    description = "Specify an orderby string to be applied to the search. Orderby strings utilize OData v4 sort syntax",
                    @default = "",
                    required = false
                });

                operation.parameters.Add(new Parameter
                {
                    name = "query",
                    @in = "query",
                    type = "string",
                    description = "Free text search to filter content. Supports query string syntax for searching in individual fields",
                    @default = "",
                    required = false
                });

                operation.parameters.Add(new Parameter
                {
                    name = "skip",
                    @in = "query",
                    type = "integer",
                    description = "Specify number of items to skip in returned search results (used for pagination)",
                    @default = "",
                    required = false
                });

                operation.parameters.Add(new Parameter
                {
                    name = "top",
                    @in = "query",
                    type = "integer",
                    description = "Specify number of items to return in search results (used for pagination)",
                    @default = "",
                    required = false
                });

                operation.parameters.Add(new Parameter
                {
                    name = "personalize",
                    @in = "query",
                    type = "boolean",
                    description = "Determines if returned content will be personalized (via the Visitor Group system) when returned. By default, content will be fetched in the context of an anonymous user.",
                    @default = "",
                    required = false
                });

                operation.parameters.Add(new Parameter
                {
                    name = "expand",
                    @in = "query",
                    type = "string",
                    description = @"Comma-separated list of reference properties (Content References, Content Areas) which should have their content fetched in the response. Passing ' * ' will load content in all reference properties in the return.",
                    @default = "",
                    required = false
                });
            }
        }
    }
}
