using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Security;
using EPiServer.SpecializedProperties;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Security;
using System;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyContentArea"/>
    /// </summary>
    public partial class ContentAreaPropertyModel : CollectionPropertyModelBase<ContentAreaItemModel, PropertyContentArea>
    {
        public ContentAreaPropertyModel(
              PropertyContentArea propertyContentArea,
              bool excludePersonalizedContent) : this(propertyContentArea, 
                  excludePersonalizedContent,
                  ServiceLocator.Current.GetInstance<ContentLoaderService>(),
                  ServiceLocator.Current.GetInstance<IContentModelMapper>(),
                  ServiceLocator.Current.GetInstance<IContentAccessEvaluator>(),
                  ServiceLocator.Current.GetInstance<ISecurityPrincipal>())
        {

        }

        public ContentAreaPropertyModel
        (
            PropertyContentArea propertyContentArea, 
            bool excludePersonalizedContent,
            ContentLoaderService contentLoaderService,
            IContentModelMapper contentModelMapper,
            IContentAccessEvaluator accessEvaluator,
			ISecurityPrincipal principalAccessor
        ) : base(propertyContentArea, excludePersonalizedContent, contentLoaderService, contentModelMapper, accessEvaluator, principalAccessor)
        {
            
        }        
    }
}
