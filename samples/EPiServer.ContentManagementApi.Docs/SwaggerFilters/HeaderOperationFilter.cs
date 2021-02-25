using System.Linq;
using System.Web.Http.Description;
using EPiServer.ContentManagementApi.Internal;
using Swashbuckle.Swagger;

namespace EPiServer.ContentManagementApi.Docs.SwaggerFilters
{
    public class HeaderOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.parameters == null)
            {
                return;
            }

            var controllerType = apiDescription.ActionDescriptor.ControllerDescriptor.ControllerType;
            if (!controllerType.Namespace.StartsWith("EPiServer.ContentManagementApi.Controllers"))
            {
                return;
            }

            // Convert query params to header params
            foreach (var queryParam in operation.parameters.Where(m => m.@in == "query"))
            {
                if (queryParam.name == "languages")
                {
                    queryParam.@in = "header";
                    queryParam.name = "Accept-Language";
                    queryParam.type = "string";
                    queryParam.collectionFormat = null;
                    queryParam.items = null;
                    queryParam.required = false;
                }

                if (queryParam.name == HeaderConstants.ValidationMode)
                {
                    queryParam.@in = "header";
                    queryParam.description = "The validation mode used when saving content.";
                }

                if (queryParam.name == HeaderConstants.PermanentDeleteHeaderName)
                {
                    queryParam.@in = "header";
                    queryParam.description = "Set to true in order to permanently delete the content. Otherwise it will be moved to the wastebasket. Read from the 'x-epi-permanent-delete' header.";
                }
            }
        }
    }
}
