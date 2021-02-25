﻿using System.Web.Mvc;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Alloy.Sample.Business.Rendering;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentManagementApi.Configuration;

namespace Alloy.Sample.Business.Initialization
{
    [InitializableModule]
    public class DependencyResolverInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            //Implementations for custom interfaces can be registered here.

            context.ConfigurationComplete += (o, e) =>
            {
                //Register custom implementations that should be used in favour of the default implementations
                context.Services.AddTransient<IContentRenderer, ErrorHandlingContentRenderer>()
                    .AddTransient<ContentAreaRenderer, AlloyContentAreaRenderer>();

                context.Services.Configure<ContentApiConfiguration>(c =>
                {
                    c.EnablePreviewFeatures = true;
                    c.Default()
                        .SetMinimumRoles(string.Empty)
                        .SetRequiredRole(string.Empty);
                });

                // Accept anonymous calls during test
                context.Services.Configure<ContentManagementApiOptions>(c =>
                {
                    c.ClearAllowedScopes().SetRequiredRole(null);
                });
            };
            
        }

        public void Initialize(InitializationEngine context)
        {
            DependencyResolver.SetResolver(new ServiceLocatorDependencyResolver(context.Locate.Advanced));
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}