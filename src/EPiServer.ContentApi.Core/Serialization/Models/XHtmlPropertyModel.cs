using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyXhtmlString"/>
    /// </summary>
    public partial class XHtmlPropertyModel : PersonalizablePropertyModel<string, PropertyXhtmlString>
    {
        protected readonly Injected<IXHtmlStringPropertyRenderer> _xHtmlStringPropertyRenderer;

        [JsonConstructor]
        internal XHtmlPropertyModel() { }

        public XHtmlPropertyModel(PropertyXhtmlString propertyXhtmlString, ConverterContext converterContext) 
            : base(propertyXhtmlString, converterContext)
        {
            Value = converterContext.IsContentManagementRequest ?
                propertyXhtmlString.XhtmlString?.ToEditString() : _xHtmlStringPropertyRenderer.Service.Render(propertyXhtmlString, converterContext.ExcludePersonalizedContent);
        }
    }
}
