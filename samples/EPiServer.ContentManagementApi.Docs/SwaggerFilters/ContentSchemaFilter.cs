using System;
using System.Collections.Generic;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentManagementApi.Models.Internal;
using EPiServer.Core;
using Swashbuckle.Swagger;

namespace EPiServer.ContentManagementApi.Docs.SwaggerFilters
{
    public class ContentSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (type == typeof(ContentApiModel))
            {
                // Set readOnly
                _readOnlyFields.ForEach(field =>
                {
                    var camelCaseField = ContentManagementApiFilter.ToCamelCase(field);
                    schema.properties[camelCaseField].readOnly = true;

                    // Readonly for property is object
                    if (field.Equals(nameof(ContentApiModel.MasterLanguage)))
                    {
                        schema.properties[camelCaseField].allOf = new List<Schema> { new Schema { @ref = schema.properties[camelCaseField].@ref } };
                        schema.properties[camelCaseField].@ref = null;
                    }
                });

                // Set required
                schema.required = _requiredFields.ConvertAll(ContentManagementApiFilter.ToCamelCase);

                // Update VersionStatus
                var enumKey = nameof(ContentApiCreateModel.Status).ToLower();
                if (schema.properties.ContainsKey(enumKey))
                {
                    schema.properties[enumKey].@enum = new string[] {
                        nameof(VersionStatus.Rejected), nameof(VersionStatus.CheckedIn), nameof(VersionStatus.CheckedOut),
                        nameof(VersionStatus.Published), nameof(VersionStatus.DelayedPublish), nameof(VersionStatus.AwaitingApproval),
                    };
                }
            }
        }

        private readonly List<string> _readOnlyFields = new List<string>() {
            nameof(ContentApiModel.Saved),
            nameof(ContentApiModel.Changed),
            nameof(ContentApiModel.Url),
            nameof(ContentApiModel.ExistingLanguages),
            nameof(ContentApiModel.MasterLanguage),
            nameof(ContentApiModel.Created),
        };

        private readonly List<string> _requiredFields = new List<string>() {
            nameof(ContentApiModel.Name),
            nameof(ContentApiModel.ContentType),
            nameof(ContentApiModel.ParentLink)
        };
    }
}
