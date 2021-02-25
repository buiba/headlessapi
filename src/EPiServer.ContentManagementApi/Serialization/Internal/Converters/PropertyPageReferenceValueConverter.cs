using System;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentManagementApi.Serialization.Internal.Converters
{
    /// <summary>
    /// The converter reponsible for converting <see cref="PageReferencePropertyModel"/> to <see cref="PageReference"/>
    /// </summary>
    [ServiceConfiguration(typeof(IPropertyDataValueConverter))]
    [PropertyDataValueConverter(typeof(PageReferencePropertyModel))]
    internal class PropertyPageReferenceValueConverter : IPropertyDataValueConverter
    {
        ///<inheritdoc />
        public object Convert(IPropertyModel propertyModel, PropertyData propertyData)
        {
            if (propertyModel is null)
            {
                throw new ArgumentNullException(nameof(propertyModel));
            }

            if (!(propertyModel is PageReferencePropertyModel pageReferencePropertyModel))
            {
                throw new NotSupportedException("PropertyPageReferenceValueConverter supports to convert PageReferencePropertyModel only");
            }

            var pageModelReference = pageReferencePropertyModel.Value;
            return ContentModelReference.IsNullOrEmpty(pageModelReference) ? null : new PageReference()
            {
                ID = pageModelReference.Id.Value,
                WorkID = pageModelReference.WorkId ?? 0,
                ProviderName = pageModelReference.ProviderName
            };
        }
    }
}
