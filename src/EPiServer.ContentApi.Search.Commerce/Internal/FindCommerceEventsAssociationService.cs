using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Search.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Search.Commerce.Internal
{
    /// <summary>
    /// Initialize the association between Catalog node events and EPiServer.Find indexer in order to keep up to date with the latest for Search.
    /// </summary>
    [ServiceConfiguration(typeof(FindEventsAssociationService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class FindCommerceEventsAssociationService : FindEventsAssociationService
    {
        public FindCommerceEventsAssociationService(
            IContentEvents contentEvents, 
            IContentSecurityRepository contentSecurityRepository, 
            IContentRepository contentRepository, 
            ContentEventIndexerWrapper contentEventIndexerWrapper) : base(
                contentEvents, 
                contentSecurityRepository, 
                contentRepository, 
                contentEventIndexerWrapper)
        {
        }

        /// <inheritdoc />
        public override void AttachContentSecurityEvent(object sender, ContentSecurityEventArg e)
        {
            var changedContent = _contentRepository.Get<IContent>(e.ContentLink);
            // Because product/variant does not have their own roles, but depend on parent role, 
            // so when update Catalog content, if the updated content is Node, we should re-index its children too
            if (changedContent is NodeContentBase)
            {
                var descendentContentLinks = _contentRepository.GetDescendents(changedContent.ContentLink);                
                foreach (var entryContentLink in descendentContentLinks)
                {
                    _contentEventIndexerWrapper.SavingContent(entryContentLink);
                }
            }

            base.AttachContentSecurityEvent(sender, e);
        }
    }
}
