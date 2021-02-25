using System.Linq;
using EPiServer.ContentApi.Search.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Find.Helpers;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Search
{
    /// <summary>
    /// Initialize the association between CMS events and EPiServer.Find indexer in order to keep up to date with the latest for Search.
    /// </summary>
    [ServiceConfiguration(typeof(FindEventsAssociationService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class FindEventsAssociationService 
    {
        protected readonly IContentEvents _contentEvents;
        protected readonly IContentSecurityRepository _contentSecurityRepository;
        protected readonly IContentRepository _contentRepository;
        protected readonly ContentEventIndexerWrapper _contentEventIndexerWrapper;

        /// <summary>
        /// We will inject this service in InitializationModule, so we need a parameterless constructor
        /// </summary>
        public FindEventsAssociationService(): this(ServiceLocator.Current.GetInstance<IContentEvents>(),
                                                          ServiceLocator.Current.GetInstance<IContentSecurityRepository>(),
                                                          ServiceLocator.Current.GetInstance<IContentRepository>(),
                                                          ServiceLocator.Current.GetInstance<ContentEventIndexerWrapper>())
        {
        }

        /// <summary>
        /// Initialize a instance of <see cref="FindEventsAssociationService"/>  with given parameters
        /// </summary>        
        public FindEventsAssociationService(
            IContentEvents contentEvents, 
            IContentSecurityRepository contentSecurityRepository,
            IContentRepository contentRepository,
            ContentEventIndexerWrapper contentEventIndexerWrapper
        )
        {
            _contentEvents = contentEvents;
            _contentSecurityRepository = contentSecurityRepository;
            _contentRepository = contentRepository;
            _contentEventIndexerWrapper = contentEventIndexerWrapper;
        }

        /// <summary>
        /// Initialize the association with EPiServer.Find events
        /// </summary>
        public virtual void Initialize()
        {
            _contentEvents.PublishedContent += CmsContentEvents;
            _contentEvents.MovedContent += CmsContentEvents;
            _contentEvents.DeletedContent += CmsContentEvents;
            _contentEvents.DeletedContentLanguage += CmsContentEvents;

            _contentSecurityRepository.ContentSecuritySaved += AttachContentSecurityEvent;
        }

        /// <summary>
        /// associate with EPiServer.Find when a content has been changed.
        /// </summary>
        public virtual void CmsContentEvents(object sender, ContentEventArgs e)
        {
            IndexContentWhichReferencesContentLink(e.ContentLink);
        }

        /// <summary>
        /// associate with EPiServer.Find when security descriptor of content has been changed.
        /// </summary>
        public virtual void AttachContentSecurityEvent(object sender, ContentSecurityEventArg e)
        {
            IndexContentWhichReferencesContentLink(e.ContentLink);
        }

        /// <summary>
        /// Raise changes to EPiServer.Find
        /// </summary>
        protected virtual void IndexContentWhichReferencesContentLink(ContentReference contentReference)
        {
            var references = _contentRepository.GetReferencesToContent(contentReference, false);

            var distinctReferences = references.Where(x => x.ReferenceType != (int)ReferenceType.ExternalReference && !x.OwnerID.Equals(contentReference, true)).DistinctBy(x => x.OwnerID);
            foreach (var reference in distinctReferences)
            {
                _contentEventIndexerWrapper.SavingContent(reference.OwnerID);
            }
        }
    }
}
