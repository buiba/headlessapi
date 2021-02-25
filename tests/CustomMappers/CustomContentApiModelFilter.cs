using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomMappers
{
    [ServiceConfiguration(typeof(IContentApiModelFilter), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class CustomContentApiModelFilter : ContentApiModelFilter<ContentApiModel>
    {
        public override void Filter(ContentApiModel contentApiModel, ConverterContext converterContext)
        {
            if (IsCustomPage(contentApiModel))
            {
                contentApiModel.ContentLink.Id = null;
                contentApiModel.ContentLink.WorkId = null;

                //since the CustomContentModelMapper implements the "old" api (to verify) to verify that it works. It does not handle flatten so we do it here instead
                if (converterContext.Options.FlattenPropertyModel)
                {
                     contentApiModel.Properties = FlattenProperties(contentApiModel.Properties);
                }
            }
        }

        private IDictionary<string, object> FlattenProperties(IDictionary<string, object> properties)
        {
            var flattendedProperties = new Dictionary<string, object>();
            foreach (var property in properties)
            {
                if (property.Value is IFlattenableProperty flattenableProperty)
                {
                    flattendedProperties[property.Key] = flattenableProperty.Flatten();
                }
                else
                {
                    flattendedProperties[property.Key] = property.Value;
                }
            }
            return flattendedProperties;
        }

        private bool IsCustomPage(ContentApiModel contentApiModel)
        {
            return contentApiModel.ContentType != null &&
                contentApiModel.ContentType.Contains(nameof(PageWithCustomHandledProperty));
        }
    }
}
