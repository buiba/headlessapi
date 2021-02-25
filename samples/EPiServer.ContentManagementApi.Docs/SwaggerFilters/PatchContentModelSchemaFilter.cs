using System;
using EPiServer.ContentManagementApi.Models.Internal;
using EPiServer.Core;
using Swashbuckle.Swagger;

namespace EPiServer.ContentManagementApi.Docs.SwaggerFilters
{
    public class PatchContentModelSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (type == typeof(ContentApiPatchModel))
            {
                // Update VersionStatus
                var enumKey = nameof(ContentApiPatchModel.Status).ToLower();
                if (schema.properties.ContainsKey(enumKey))
                {
                    schema.properties[enumKey].@enum = new string[] {
                        nameof(VersionStatus.Rejected), nameof(VersionStatus.CheckedIn), nameof(VersionStatus.CheckedOut),
                        nameof(VersionStatus.Published), nameof(VersionStatus.DelayedPublish), nameof(VersionStatus.AwaitingApproval),
                    };
                }
            }
        }
    }
}
