using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;
using EPiServer.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EPiServer.ContentApi.Core.Tracking.Internal
{
    /// <summary>
    /// Filter reponsible for adding tracking logic when converting IContent
    /// </summary>
    [ServiceConfiguration(typeof(IContentFilter), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class TrackingContentFilter : ContentFilter<IContent>
    {
        private readonly IContentApiTrackingContextAccessor _contentApiTrackingContextAccessor;
        private readonly ContentApiAuthorizationService _contentApiAuthorizationService;
        private readonly IPermanentLinkMapper _permanentLinkMapper;

        public TrackingContentFilter(
            IContentApiTrackingContextAccessor contentApiTrackingContextAccessor,
            ContentApiAuthorizationService contentApiAuthorizationService,
            IPermanentLinkMapper permanentLinkMapper)
        {
            _contentApiTrackingContextAccessor = contentApiTrackingContextAccessor;
            _contentApiAuthorizationService = contentApiAuthorizationService;
            _permanentLinkMapper = permanentLinkMapper;
        }

        public override void Filter(IContent content, ConverterContext converterContext)
        {
            var currentContext = _contentApiTrackingContextAccessor.Current;              

            DateTime? expirationTime = null;
            if (content is IVersionable versionable && versionable.StopPublish.HasValue)
            {
                expirationTime = versionable.StopPublish.Value;
            }

            DateTime? savedTime = null;
            if (content is IChangeTrackable changeTrackable)
            {
                savedTime = changeTrackable.Saved;
            }

            var currentReferencedContentMetadata = TrackReferencedContent(currentContext, content.ContentLink, (content as ILocale)?.Language, new ReferencedContentMetadata { ExpirationTime = expirationTime, SavedTime = savedTime });
            EvaluateAndTrackPotentialSecuredContent(currentContext, content);

            // With PageData, the ParentLink is included in IContentData.PropertyData
            // With Block/Folder/Media, The ParentLink is from IContent.ParentLink
            if (!ContentReference.IsNullOrEmpty(content.ParentLink))
            {
                TrackReferencedContent(currentContext, content.ParentLink);
            }

            // Content Api only returns non-metadata properties, so here we should not tracking metadata properties
            foreach (var property in content.Property.Where(p => !p.IsNull && !p.IsMetaData))
            {
                // For xhtmlstring/contentarea we check regardless if they are expanded since they are always filtered, for other properties we only track if they are expanded (otherwise they are not filtered)
                switch (property.Value)
                {
                    case XhtmlString xhtmlString:
                        if (IsPersonalized(xhtmlString))
                        {
                            currentReferencedContentMetadata.PersonalizedProperties.Add(property.Name);
                        }
                        foreach (var fragment in xhtmlString.Fragments)
                        {
                            if (fragment is ContentFragment contentFragment)
                            {
                                EvaluateAndTrackPotentialSecuredContent(currentContext, contentFragment.ContentLink);
                                TrackReferencedContent(currentContext, contentFragment.ContentLink, (contentFragment.GetContent() as ILocale)?.Language);
                            }
                            else if (fragment is UrlFragment urlFragment)
                            {
                                EvaluateAndTrackReferencedContentInUrl(currentContext, urlFragment.InternalFormat);
                            }
                        }
                        break;
                    case ContentReference contentReference:
                        if (converterContext.ShouldExpand(property.Name))
                        {
                            EvaluateAndTrackPotentialSecuredContent(currentContext, contentReference);
                        }
                        TrackReferencedContent(currentContext, contentReference);
                        break;
                    case IEnumerable<ContentReference> contentReferenceList:
                        foreach (var contentReference in contentReferenceList)
                        {
                            if (converterContext.ShouldExpand(property.Name))
                            {
                                EvaluateAndTrackPotentialSecuredContent(currentContext, contentReference);
                            }
                            TrackReferencedContent(currentContext, contentReference);
                        }
                        break;
                    case LinkItemCollection linkItems:
                        foreach (var linkItem in linkItems)
                        {
                            if (converterContext.ShouldExpand(property.Name))
                            {
                                EvaluateAndTrackPotentialSecuredContent(currentContext, GetLanguageContentReference(linkItem.Href)?.ContentLink);
                            }
                            EvaluateAndTrackReferencedContentInUrl(currentContext, linkItem.Href);
                        }
                        break;
                    case Url url:
                        {
                            EvaluateAndTrackReferencedContentInUrl(currentContext, url.ToString());
                            break;
                        }
                }
            }
        }

        private bool IsPersonalized(XhtmlString xhtmlString) => xhtmlString is ContentArea contentArea ?
            IsPersonalized(contentArea) :
            xhtmlString.Fragments.OfType<PersonalizedContentFragment>().Any();

        private bool IsPersonalized(ContentArea contentArea) => contentArea.Items.Any(i => (i.AllowedRoles ?? Enumerable.Empty<string>()).Any());

        private void EvaluateAndTrackPotentialSecuredContent(ContentApiTrackingContext trackingContext, IContent content)
        {
            if (content is object && !trackingContext.SecuredContent.Contains(content.ContentLink) && !_contentApiAuthorizationService.IsAnonymousAllowedToAccessContent(content))
            {
                trackingContext.SecuredContent.Add(content.ContentLink);
            }
        }

        private void EvaluateAndTrackPotentialSecuredContent(ContentApiTrackingContext trackingContext, ContentReference contentLink)
        {
            if (!ContentReference.IsNullOrEmpty(contentLink) && !trackingContext.SecuredContent.Contains(contentLink) && !_contentApiAuthorizationService.IsAnonymousAllowedToAccessContent(contentLink))
            {
                trackingContext.SecuredContent.Add(contentLink);
            }
        }

        private ReferencedContentMetadata TrackReferencedContent(ContentApiTrackingContext trackingContext, ContentReference contentLink, CultureInfo language = null, ReferencedContentMetadata referenceContentMetadata = null)
        {            
            if (ContentReference.IsNullOrEmpty(contentLink))
            {
                return null;
            }

            var languageContentReference = new LanguageContentReference(contentLink, language);
            if (trackingContext.ReferencedContent.TryGetValue(languageContentReference, out var existingContentMetadata))
            {       
                // In case the existing metadata is null, we should update the new metadata for the current languageContentReference
                if (existingContentMetadata == null && referenceContentMetadata != null)
                {
                    trackingContext.ReferencedContent[languageContentReference] = referenceContentMetadata;
                }

                return trackingContext.ReferencedContent[languageContentReference];
            }

            trackingContext.ReferencedContent.Add(languageContentReference, referenceContentMetadata);
            return referenceContentMetadata;
        }

        private void EvaluateAndTrackReferencedContentInUrl(ContentApiTrackingContext trackingContext, string url)
        {
            var languageContentReference = GetLanguageContentReference(url);

            // we need to check here to make sure that given url is pointed to a valid content
            if (languageContentReference is object)
            {
                TrackReferencedContent(trackingContext, languageContentReference.ContentLink, languageContentReference.Language);
            }
        }

        // Get the LanguageContentReference from the url. Returns null if the url is not pointed to any content              
        private LanguageContentReference GetLanguageContentReference(string url)
        {
            var urlBuilder = new UrlBuilder(url);
            var contentGuid = PermanentLinkUtility.GetGuid((string)urlBuilder);

            // In case url is not pointed to IContent (ex: external url), we return null early here
            if (contentGuid == Guid.Empty)
            {
                return null;
            }

            var contentReference = _permanentLinkMapper.Find(contentGuid)?.ContentReference;
            // in case linked content is deleted, the contentReference might be null, so we need to check here
            if (ContentReference.IsNullOrEmpty(contentReference))
            {
                return null;
            }
           
            return new LanguageContentReference(contentReference, urlBuilder.QueryLanguage != null ? new CultureInfo(urlBuilder.QueryLanguage) : null);
        }
    }
}