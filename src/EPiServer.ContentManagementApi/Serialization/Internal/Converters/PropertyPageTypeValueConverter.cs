using System;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentManagementApi.Serialization.Internal.Converters
{
    [ServiceConfiguration(typeof(IPropertyDataValueConverter))]
    [PropertyDataValueConverter(typeof(PageTypePropertyModel))]
    internal class PropertyPageTypeValueConverter : IPropertyDataValueConverter
    {
        private readonly IContentTypeRepository _contentTypeRepository;

        internal PropertyPageTypeValueConverter() : this(ServiceLocator.Current.GetInstance<IContentTypeRepository>())
        {
        }

        public PropertyPageTypeValueConverter(IContentTypeRepository contentTypeRepository)
        {
            _contentTypeRepository = contentTypeRepository;
        }

        public object Convert(IPropertyModel propertyModel, PropertyData _)
        {
            if (propertyModel is null)
            {
                throw new ArgumentNullException(nameof(propertyModel));
            }

            if (!(propertyModel is PageTypePropertyModel pageTypePropertyModel))
            {
                throw new NotSupportedException("PropertyPageTypeValueConverter supports to convert PageTypePropertyModel only");
            }

            if (string.IsNullOrEmpty(pageTypePropertyModel.Value))
            {
                return null;
            }

            var pageType = _contentTypeRepository.Load(pageTypePropertyModel.Value);
            return pageType?.ID;
        }
    }
}
