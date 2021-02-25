using System;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentManagementApi.Serialization.Internal.Converters
{
    /// <summary>
    /// The converter reponsible for converting <see cref="XHtmlPropertyModel"/> to <see cref="XhtmlString"/>
    /// </summary>
    [ServiceConfiguration(typeof(IPropertyDataValueConverter))]
    [PropertyDataValueConverter(typeof(XHtmlPropertyModel))]
    internal class PropertyXhtmlStringValueConverter : IPropertyDataValueConverter
    {
        ///<inheritdoc />
        public object Convert(IPropertyModel propertyModel, PropertyData propertyData)
        {
            if (propertyModel == null)
            {
                throw new ArgumentNullException(nameof(propertyModel));
            }

            if (!(propertyModel is XHtmlPropertyModel xHtmlPropertyModel))
            {
                throw new NotSupportedException("PropertyXhtmlStringValueConverter supports to convert XHtmlPropertyModel only");
            }

            return new XhtmlString(xHtmlPropertyModel.Value);
        }
    }
}
