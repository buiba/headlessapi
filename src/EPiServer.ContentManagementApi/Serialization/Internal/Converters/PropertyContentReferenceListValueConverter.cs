using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentManagementApi.Serialization.Internal.Converters
{
    /// <summary>
    /// The converter reponsible for converting <see cref="ContentReferenceListPropertyModel"/> to <see cref="List{ContentReference}"/>
    /// </summary>
    [ServiceConfiguration(typeof(IPropertyDataValueConverter))]
    [PropertyDataValueConverter(typeof(ContentReferenceListPropertyModel))]
    internal class PropertyContentReferenceListValueConverter : IPropertyDataValueConverter
    {
        ///<inheritdoc />
        public object Convert(IPropertyModel propertyModel, PropertyData propertyData)
        {
            if (propertyModel == null)
            {
                throw new ArgumentNullException(nameof(propertyModel));
            }

            if (!(propertyModel is ContentReferenceListPropertyModel contentReferenceListPropertyModel))
            {
                throw new NotSupportedException("PropertyContentReferenceListValueConverter supports to convert ContentReferenceListPropertyModel only");
            }

            var contentReferences = contentReferenceListPropertyModel.Value?.Where(c => !ContentModelReference.IsNullOrEmpty(c)).Select(x => new ContentReference()
            {
                ID = x.Id.Value,
                WorkID = x.WorkId ?? 0,
                ProviderName = x.ProviderName,
            });

            return contentReferences is object ? new List<ContentReference>(contentReferences) : null;
        }
    }
}
