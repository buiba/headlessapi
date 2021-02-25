using EPiServer.ContentApi.Core.Configuration;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using System;
using System.Configuration;

namespace EPiServer.ContentApi.Core.OutputCache.Internal
{
    [InitializableModule]
    internal class OutputCacheInitializationModule : IConfigurableModule
    {
        private IContentEvents _contentEvents;
        private IContentSecurityRepository _contentSecurityRepository;
        private ContentDependencyPropagator _contentDependencyPropagator;
        private SiteDependencyPropagator _siteDependencyPropagator;
        private ISiteDefinitionEvents _siteDefinitionEvents;
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton<IOutputCacheEvaluator, ContentOutputCacheEvaluator>();
            context.Services.AddSingleton<IOutputCacheEvaluator, SiteOutputCacheEvaluator>();
            context.Services.AddSingleton<IOutputCacheEvaluator, ChildrenContentOutputCacheEvaluator>();
            context.Services.AddSingleton<IOutputCacheEvaluator, AncestorsContentOutputCacheEvaluator>();
            context.Services.AddSingleton<IOutputCacheProvider, DefaultOutputCacheProvider>();
                   
            if (int.TryParse(ConfigurationManager.AppSettings["episerver:contentdelivery:httpresponseexpiretime"], out int httpResponseExpireTime))
            {
                context.Services.Configure<ContentApiConfiguration>(config => config.Default().SetHttpResponseExpireTime(TimeSpan.FromSeconds(httpResponseExpireTime)));
            }
        }

        public void Initialize(InitializationEngine context)
        {
            _contentEvents = context.Locate.Advanced.GetInstance<IContentEvents>();
            _contentSecurityRepository = context.Locate.Advanced.GetInstance<IContentSecurityRepository>();
            _siteDefinitionEvents = context.Locate.Advanced.GetInstance<ISiteDefinitionEvents>();
            _contentDependencyPropagator = context.Locate.Advanced.GetInstance<ContentDependencyPropagator>();
            _siteDependencyPropagator = context.Locate.Advanced.GetInstance<SiteDependencyPropagator>();

            _contentEvents.MovedContent += _contentDependencyPropagator.MovedContent;
            _contentEvents.PublishingContent += _contentDependencyPropagator.PublishingContent;
            _contentEvents.PublishedContent += _contentDependencyPropagator.PublishedContent;
            _contentEvents.DeletingContent += _contentDependencyPropagator.DeletingContent;
            _contentEvents.DeletedContent += _contentDependencyPropagator.DeletedContent;
            _contentEvents.DeletedContentLanguage += _contentDependencyPropagator.DeletedContentLanguage;

            _contentSecurityRepository.ContentSecuritySaved += _contentDependencyPropagator.ContentSecuritySaved;

            _siteDefinitionEvents.SiteCreated += _siteDependencyPropagator.SiteDefinitionCreated;
            _siteDefinitionEvents.SiteUpdated += _siteDependencyPropagator.SiteDefinitionUpdated;
            _siteDefinitionEvents.SiteDeleted += _siteDependencyPropagator.SiteDefinitionDeleted;
        }

        public void Uninitialize(InitializationEngine context)
        {
            _contentEvents.MovedContent -= _contentDependencyPropagator.MovedContent;
            _contentEvents.PublishingContent -= _contentDependencyPropagator.PublishingContent;
            _contentEvents.PublishedContent -= _contentDependencyPropagator.PublishedContent;
            _contentEvents.DeletingContent -= _contentDependencyPropagator.DeletingContent;
            _contentEvents.DeletedContent -= _contentDependencyPropagator.DeletedContent;
            _contentEvents.DeletedContentLanguage -= _contentDependencyPropagator.DeletedContentLanguage;

            _contentSecurityRepository.ContentSecuritySaved -= _contentDependencyPropagator.ContentSecuritySaved;

            _siteDefinitionEvents.SiteCreated -= _siteDependencyPropagator.SiteDefinitionCreated;
            _siteDefinitionEvents.SiteUpdated -= _siteDependencyPropagator.SiteDefinitionUpdated;
            _siteDefinitionEvents.SiteDeleted -= _siteDependencyPropagator.SiteDefinitionDeleted;
        }
    }
}
