using System;
using System.Linq;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentManagementApi.Serialization.Internal.Converters
{
    /// <summary>
    /// The converter reponsible for converting <see cref="CategoryPropertyModel"/> to <see cref="CategoryList"/>
    /// </summary>
    [ServiceConfiguration(typeof(IPropertyDataValueConverter))]
    [PropertyDataValueConverter(typeof(CategoryPropertyModel))]
    internal class PropertyCategoryValueConverter : IPropertyDataValueConverter
    {
        ///<inheritdoc />
        public object Convert(IPropertyModel propertyModel, PropertyData propertyData)
        {
            if (propertyModel == null)
            {
                throw new ArgumentNullException(nameof(propertyModel));
            }

            if (!(propertyModel is CategoryPropertyModel))
            {
                throw new NotSupportedException("PropertyCategoryValueConverter supports to convert CategoryPropertyModel only");
            }

            var categoryPropertyModel = propertyModel as CategoryPropertyModel;
            var categories = categoryPropertyModel.Value?.Select(x => x.Id);

            return categories is object ? new CategoryList(categories) : null;
        }
    }
}
