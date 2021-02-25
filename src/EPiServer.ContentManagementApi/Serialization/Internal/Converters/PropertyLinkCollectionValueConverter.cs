using System;
using System.Linq;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;

namespace EPiServer.ContentManagementApi.Serialization.Internal.Converters
{
    /// <summary>
    /// The converter reponsible for converting simple value <see cref="LinkCollectionPropertyModel"/> to <see cref="LinkItemCollection"/>
    /// </summary>
    [ServiceConfiguration(typeof(IPropertyDataValueConverter))]
    [PropertyDataValueConverter(typeof(LinkCollectionPropertyModel))]
    internal class PropertyLinkCollectionValueConverter : IPropertyDataValueConverter
    {
        ///<inheritdoc />
        public object Convert(IPropertyModel propertyModel, PropertyData propertyData)
        {
            if (propertyModel == null)
            {
                throw new ArgumentNullException(nameof(propertyModel));
            }

            if (!(propertyModel is LinkCollectionPropertyModel))
            {
                throw new NotSupportedException("PropertyLinkCollectionValueConverter supports to convert LinkCollectionPropertyModel only");
            }
            
            var linkCollectionPropertyModel = propertyModel as LinkCollectionPropertyModel;
            var linkItems = linkCollectionPropertyModel.Value?.Select(x => new LinkItem()
            {
                Href = x.Href,
                Target = x.Target,
                Text = x.Text,
                Title = x.Title
            });

            return linkItems is object ? new LinkItemCollection(linkItems) : null;
        }
    }
}
