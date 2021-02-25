using EPiServer.Core;
using EPiServer.PlugIn;

namespace EPiServer.ContentApi.IntegrationTests.ContentModels.Properties
{
    [PropertyDefinitionTypePlugIn(Description = "A custom PropertyLongString", DisplayName = "String List")]
    public class CustomPropertyLongString : PropertyLongString
    {
    }
}
