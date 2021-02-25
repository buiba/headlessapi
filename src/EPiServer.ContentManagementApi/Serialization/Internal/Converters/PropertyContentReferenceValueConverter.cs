using System;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentManagementApi.Serialization.Internal.Converters
{
    /// <summary>
    /// The converter reponsible for converting <see cref="ContentReferencePropertyModel"/> to <see cref="ContentReference"/>
    /// </summary>
    [ServiceConfiguration(typeof(IPropertyDataValueConverter))]
    [PropertyDataValueConverter(typeof(ContentReferencePropertyModel))]
    internal class PropertyContentReferenceValueConverter : IPropertyDataValueConverter
    {
        ///<inheritdoc />
        public object Convert(IPropertyModel propertyModel, PropertyData propertyData = null)
        {
            if (propertyModel == null)
            {
                throw new ArgumentNullException(nameof(propertyModel));
            }

            if (!(propertyModel is ContentReferencePropertyModel contentReferencePropertyModel))
            {
                throw new NotSupportedException("PropertyContentReferenceValueConverter supports to convert ContentReferencePropertyModel only");
            }
            
            var contentModelReference = contentReferencePropertyModel.Value;
            return ContentModelReference.IsNullOrEmpty(contentModelReference) ? null : new ContentReference()
            {
                ID = contentModelReference.Id.Value,
                WorkID = contentModelReference.WorkId ?? 0,
                ProviderName = contentModelReference.ProviderName
            };       
        }
    }
}
