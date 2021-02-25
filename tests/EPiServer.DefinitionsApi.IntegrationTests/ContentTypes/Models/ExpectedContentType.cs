using System;
using System.Collections.Generic;
using EPiServer.DefinitionsApi.ContentTypes;
using EPiServer.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EPiServer.DefinitionsApi.IntegrationTests.ContentTypes.Models
{
    // These could be converted to Records when we move to C# 9 and .NET 5.0
    internal class ExpectedContentType
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string BaseType { get; set; }
        public string Version { get; set; }
        public ExpectedContentTypeEditSettings EditSettings { get; set; }
        public List<ExpectedProperty> Properties { get; set; }
    }

    internal class ExpectedProperty
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public string ItemType { get; set; }
        public bool? BranchSpecific { get; set; }
        public ExpectedPropertyEditSettings EditSettings { get; set; }
        public List<ExpectedPropertyValidationSettings> Validation { get; set; }
    }

    internal class ExpectedContentTypeEditSettings
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool? Available { get; set; }
        public int? Order { get; set; }
    }

    internal class ExpectedPropertyEditSettings
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public VisibilityStatus? Visibility { get; set; }
        public string DisplayName { get; set; }
        public string GroupName { get; set; }
        public int? Order { get; set; }
        public string HelpText { get; set; }
        public string Hint { get; set; }
    }

    internal class ExpectedPropertyValidationSettings
    {
        public string Name { get; set; }
        public ValidationErrorSeverity? Severity { get; set; }
        public string ErrorMessage { get; set; }
        [JsonExtensionData]
        public Dictionary<string, object> Settings { get; set; }
    }

}
