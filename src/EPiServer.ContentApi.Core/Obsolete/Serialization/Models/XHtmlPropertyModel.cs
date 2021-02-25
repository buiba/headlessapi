using EPiServer.ContentApi.Core.Obsolete.Serialization;
using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;
using System;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyXhtmlString"/>
    /// </summary>
    public partial class XHtmlPropertyModel : PersonalizablePropertyModel<string, PropertyXhtmlString>
    {
        public XHtmlPropertyModel(PropertyXhtmlString propertyXhtmlString, bool excludePersonalizedContent) 
            : this(propertyXhtmlString, ConverterContextFactory.ForObsolete(excludePersonalizedContent))
        {
        }
    }
}
