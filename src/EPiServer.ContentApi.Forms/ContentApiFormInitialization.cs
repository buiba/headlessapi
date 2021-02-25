using EPiServer.ContentApi.Cms;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.ContentApi.Forms.Internal;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace EPiServer.ContentApi.Forms
{
    /// <summary>
    /// Initialize default settings for Content Delivery Form
    /// </summary>
    [InitializableModule]
    [ModuleDependency(typeof(ContentApiCmsInitialization))]
    public class ContentApiFormInitialization : IConfigurableModule, IInitializableModule
    {
        /// <inheritdoc />
        public void ConfigureContainer(ServiceConfigurationContext context)
        {           
            context.Services.Intercept<IPageRouteHelper>((locator, defaultPageRouteHelper) =>
                new ExtendedPageRouteHelper(defaultPageRouteHelper, 
                    locator.GetInstance<FormRenderingService>())
            );

            context.Services.AddTransient<XhtmlRenderService, FormXhtmlRenderService>();
        }       
       
        /// <inheritdoc />
        public void Initialize(InitializationEngine context)
        {            
        }

        /// <inheritdoc />
        public void Uninitialize(InitializationEngine context)
        {
        }             
    }
}
