using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPiServer.ContentManagementApi.Serialization.Internal.Converters
{
    /// <summary>
    /// The converter reponsible for converting <see cref="ContentAreaPropertyModel"/> to <see cref="ContentArea"/>
    /// </summary>
    [ServiceConfiguration(typeof(IPropertyDataValueConverter))]
    [PropertyDataValueConverter(typeof(ContentAreaPropertyModel))]
    internal class PropertyContentAreaValueConverter : IPropertyDataValueConverter
    {
        private readonly DisplayOptions _displayOptions;
        private static readonly string ContentDisplayOptionAttributeName = ContentFragment.ContentDisplayOptionAttributeName;

        public PropertyContentAreaValueConverter(DisplayOptions displayOptions)
        {
            _displayOptions = displayOptions;
        }

        ///<inheritdoc />
        public object Convert(IPropertyModel propertyModel, PropertyData propertyData)
        {
            if (propertyModel == null)
            {
                throw new ArgumentNullException(nameof(propertyModel));
            }

            if (!(propertyModel is ContentAreaPropertyModel contentAreaPropertyModel))
            {
                throw new NotSupportedException("PropertyContentAreaValueConverter supports to convert ContentAreaPropertyModel only");
            }

            if (contentAreaPropertyModel.Value is null)
            {
                return null;
            }

            var contentArea = new ContentArea();
            foreach (var contentAreaItemModel in contentAreaPropertyModel.Value.Where(c => !ContentModelReference.IsNullOrEmpty(c.ContentLink)))
            {
                var contentAreaItem = new ContentAreaItem()
                {
                    ContentLink = new ContentReference(contentAreaItemModel.ContentLink.Id.Value, contentAreaItemModel.ContentLink.WorkId ?? 0, contentAreaItemModel.ContentLink.ProviderName),
                    RenderSettings = new Dictionary<string, object>(),
                };

                var displayOption = _displayOptions.Get(contentAreaItemModel.DisplayOption);

                if (displayOption is null && !string.IsNullOrWhiteSpace(contentAreaItemModel.DisplayOption))
                {
                    throw new ErrorException(HttpStatusCode.BadRequest, $"The provided display option with the id '{contentAreaItemModel.DisplayOption}' does not exist");
                }

                if (displayOption is object)
                {
                    contentAreaItem.RenderSettings.Add(ContentDisplayOptionAttributeName, displayOption.Id);
                }

                contentArea.Items.Add(contentAreaItem);
            }

            return contentArea;
        }
    }
}
