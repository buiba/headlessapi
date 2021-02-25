using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Security;
using EPiServer.SpecializedProperties;
using EPiServer.ContentApi.Core.Security;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyContentArea"/>
    /// </summary>
    public partial class ContentAreaPropertyModel : CollectionPropertyModelBase<ContentAreaItemModel, PropertyContentArea>
    {
        [JsonConstructor]
        internal ContentAreaPropertyModel()
        {
        }

        public ContentAreaPropertyModel(
              PropertyContentArea propertyContentArea,
              ConverterContext converterContext) : base(propertyContentArea, converterContext)
        {
        }

        public ContentAreaPropertyModel
        (
            PropertyContentArea propertyContentArea, 
            ConverterContext converterContext,
            ContentLoaderService contentLoaderService,
            ContentConvertingService contentConvertingService,
            IContentAccessEvaluator accessEvaluator,
			ISecurityPrincipal principalAccessor
        ) : base(propertyContentArea, converterContext, contentLoaderService, contentConvertingService, accessEvaluator, principalAccessor)
        {          
        }

        private IEnumerable<ContentAreaItem> FilteredItems(ContentArea contentArea, bool excludePersonalizedContent)
        {
            if (ConverterContext.ShouldIncludeAllPersonalizedContent)
            {
                return contentArea.Fragments.OfType<ContentFragment>().Select(f => new ContentAreaItem(f));
            }

            var principal = excludePersonalizedContent ? _principalAccessor.GetAnonymousPrincipal() : _principalAccessor.GetCurrentPrincipal();
            return contentArea.Fragments.GetFilteredFragments(principal).OfType<ContentFragment>().Select(f => new ContentAreaItem(f));
        }

        /// <inheritdoc />
        protected override IEnumerable<ContentAreaItemModel> GetValue()
        {
            var contentArea = _propertyLongString.Value as ContentArea;
            if (contentArea == null)
            {
                return null;
            }

            return FilteredItems(contentArea, _excludePersonalizedContent).Select(x => new ContentAreaItemModel
            {
                ContentLink = new ContentModelReference
                {
                    GuidValue = x.ContentGuid,
                    Id = x.ContentLink.ID,
                    WorkId = x.ContentLink.WorkID,
                    ProviderName = x.ContentLink.ProviderName
                },
                DisplayOption = x.RenderSettings.ContainsKey(ContentFragment.ContentDisplayOptionAttributeName)
                ? x.RenderSettings[ContentFragment.ContentDisplayOptionAttributeName].ToString() : ""
            }).ToList();
        }
    }
}
