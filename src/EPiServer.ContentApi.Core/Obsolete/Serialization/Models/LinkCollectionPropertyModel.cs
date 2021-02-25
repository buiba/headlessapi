using System.Collections.Generic;
using System.Linq;
using EPiServer.Security;
using EPiServer.SpecializedProperties;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyLinkCollection"/>
    /// </summary>
    public partial class LinkCollectionPropertyModel : CollectionPropertyModelBase<LinkItemNode, PropertyLinkCollection> 
    {
        public LinkCollectionPropertyModel
        (
            PropertyLinkCollection propertyLinkCollection, 
            bool excludePersonalizedContent)  
            : this(propertyLinkCollection, excludePersonalizedContent, ServiceLocator.Current.GetInstance<ContentLoaderService>(), ServiceLocator.Current.GetInstance<IContentModelMapper>(), ServiceLocator.Current.GetInstance<IContentAccessEvaluator>(), ServiceLocator.Current.GetInstance<ISecurityPrincipal>())
        {

        }

        public LinkCollectionPropertyModel(PropertyLinkCollection propertyLinkCollection, bool excludePersonalizedContent,
			ContentLoaderService contentLoaderService,
            IContentModelMapper contentModelMapper,
            IContentAccessEvaluator accessEvaluator,
			ISecurityPrincipal principalAccessor) : base(propertyLinkCollection, excludePersonalizedContent, contentLoaderService, contentModelMapper, accessEvaluator, principalAccessor)
        {

        }       
    }
}
