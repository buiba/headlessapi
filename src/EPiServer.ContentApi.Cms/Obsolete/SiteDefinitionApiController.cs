using System;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.ContentResult;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Tracking;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPiServer.ContentApi.Cms.Controllers
{
    public partial class SiteDefinitionApiController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteDefinitionApiController"/> class.
        /// </summary>
        [Obsolete("Use alternative constructor")]
        public SiteDefinitionApiController(IContentLoader contentLoader,
                                        ContentRootRepository contentRootRepository,
                                        ISiteDefinitionRepository siteDefinitionRepository,
                                        IContentModelReferenceConverter contentModelService,
                                        ContentApiConfiguration apiConfig)
         : this(siteDefinitionRepository,
                ServiceLocator.Current.GetInstance<ServiceAccessor<SiteDefinition>>(),
                apiConfig,
                ServiceLocator.Current.GetInstance<ISiteDefinitionConverter>(),
                ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>(),
                ServiceLocator.Current.GetInstance<ContentApiSerializerResolver>())
        { }

        [Obsolete("Use alternative constructor")]
        public SiteDefinitionApiController(
            IContentLoader contentLoader,
            ContentRootRepository contentRootRepository,
            ISiteDefinitionRepository siteDefinitionRepository,
            IContentModelReferenceConverter contentModelService,
            ContentApiConfiguration apiConfig,
            OptionsResolver optionsResolver) 
            : this(
                siteDefinitionRepository,
                ServiceLocator.Current.GetInstance<ServiceAccessor<SiteDefinition>>(),
                apiConfig,
                ServiceLocator.Current.GetInstance<ISiteDefinitionConverter>(),
                ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>(),
                ServiceLocator.Current.GetInstance<ContentApiSerializerResolver>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteDefinitionApiController"/> class.
        /// </summary>
        [Obsolete("Use alternative constructor")]
        public SiteDefinitionApiController(IContentLoader contentLoader,
                                           ContentRootRepository contentRootRepository,
                                           ISiteDefinitionRepository siteDefinitionRepository,
                                           IContentModelReferenceConverter contentModelService,
                                           ContentApiConfiguration apiConfig,
                                           OptionsResolver optionsResolver,
                                           ILanguageBranchRepository languageBranchRepository,
                                           IContentApiTrackingContextAccessor contentApiTrackingContextAccessor)
            : this(siteDefinitionRepository,
                  ServiceLocator.Current.GetInstance<ServiceAccessor<SiteDefinition>>(),
                  apiConfig,
                  ServiceLocator.Current.GetInstance<ISiteDefinitionConverter>(),
                  ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>(),
                  ServiceLocator.Current.GetInstance<ContentApiSerializerResolver>())
        {
        }
    }
}
