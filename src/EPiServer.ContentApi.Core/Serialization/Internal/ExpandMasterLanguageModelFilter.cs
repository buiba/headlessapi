using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.ContentApi.Core.Serialization.Internal
{
    /// <summary>
    /// A content item may not be expanded due to access rights and/or language settings.
    /// In edit mode we should expand them anyway so editors gets a hint that the content e.g. needs translation.
    /// 
    /// Content items that were not expanded should also be removed from the property value in view/default mode.
    /// </summary>
    [ServiceConfiguration(typeof(IContentApiModelFilter), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ExpandMasterLanguageModelFilter : ContentApiModelFilter<ContentApiModel>
    {
        private readonly ContentLoaderService _contentLoaderService;
        private readonly Injected<ContentConvertingService> _contentConvertingService;

        public ExpandMasterLanguageModelFilter(ContentLoaderService contentLoaderService)
        {
            _contentLoaderService = contentLoaderService;
        }

        public override void Filter(ContentApiModel contentApiModel, ConverterContext converterContext)
        {
            var keys = new List<string>(contentApiModel.Properties.Keys);

            foreach (var key in keys)
            {
                if (!converterContext.ShouldExpand(key))
                {
                    continue;
                }

                var propertyValue = contentApiModel.Properties[key];
               
                if (propertyValue is IContentItem contentItem)
                {
                    if (TryExpandMasterLanguage(contentItem, converterContext, out var expandedModel))
                    {
                        contentItem.ContentLink.Expanded = expandedModel;
                        contentApiModel.Properties[key] = contentItem;
                    }
                    else
                    {
                        // Content should have been expanded but we couldn't, therefor remove it.
                        contentApiModel.Properties[key] = null;
                    }
                }

                if (propertyValue is IEnumerable<IContentItem> contentItems)
                {
                    var value = new List<IContentItem>();

                    foreach (var item in contentItems)
                    {
                        if (TryExpandMasterLanguage(item, converterContext, out var expandedModel))
                        {
                            item.ContentLink.Expanded = expandedModel;

                            value.Add(item);
                        }
                    }

                    if (value.Any())
                    {
                        contentApiModel.Properties[key] = value;
                    }
                    else
                    {
                        // Non of the content items were expanded, therefor remove them.
                        contentApiModel.Properties[key] = Enumerable.Empty<object>();
                    }
                }
            }
        }

        private bool TryExpandMasterLanguage(IContentItem contentItem, ConverterContext converterContext, out ContentApiModel expandedModel)
        {
            expandedModel = contentItem.ContentLink.Expanded;

            // Content item is already expanded, no need to process further
            if (expandedModel != null)
            {
                return true;
            }

            if (converterContext.ContextMode == ContextMode.Edit)
            {
                var content = _contentLoaderService.Get(
                    contentItem.ContentLink.GuidValue.GetValueOrDefault(),
                    converterContext.Language.Name,
                    fallbackToMaster: true);

                if (content == null)
                {
                    return false;
                }

                expandedModel = _contentConvertingService.Service.ConvertToContentApiModel(content, new ConverterContext(converterContext));

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
