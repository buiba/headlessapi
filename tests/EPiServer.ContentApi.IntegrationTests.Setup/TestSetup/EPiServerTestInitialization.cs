using System;
using System.Reflection;
using System.Web.Routing;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.Core;
using EPiServer.Data;
using EPiServer.DataAbstraction;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPiServer.ContentApi.IntegrationTests.TestSetup
{
    [InitializableModule]
    [ModuleDependency(typeof(FrameworkAspNetInitialization))]
    internal class EPiServerTestInitialization : IConfigurableModule
    {
        public static string CmsDatabase { get; set; }

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            //register noop provider to avoid default membership impl
            context.Services.AddSingleton<SecurityEntityProvider, NoopSecurityEntityProvider>();
            context.Services.Configure<DataAccessOptions>(o =>
            {
                o.UpdateDatabaseSchema = true;
                o.SetConnectionString($@"Data Source=(LocalDb)\MSSQLLocalDB;Database={CmsDatabase}; Connection Timeout=60;Integrated Security=True;MultipleActiveResultSets=True");
            });
        }

        public void Initialize(InitializationEngine context)
        {
            //Create a site
            var startPage = context.Locate.ContentRepository().GetDefault<StartPage>(ContentReference.RootPage);
            startPage.Name = "Start";
            startPage.ContentGuid = IntegrationTestCollection.StartPageGuId;
            var startPageLink = context.Locate.ContentRepository().Save(startPage, DataAccess.SaveAction.Publish, AccessLevel.NoAccess);

            var siteDefinition = new SiteDefinition
            {
                Name = "Integration test site",
                Id = IntegrationTestCollection.DefaultSiteId,
                SiteUrl = new Uri("http://localhost/"),
                StartPage = startPageLink
            };
            siteDefinition.Hosts.Add(new HostDefinition { Name = HostDefinition.WildcardHostName });
            context.Locate.Advanced.GetInstance<ISiteDefinitionRepository>().Save(siteDefinition);

            //Setup routing (needs to be done since integrationtest app is not a proper aspnet web app)
            UrlRewriteContext.InitializeLanguageResolving(context.Locate.Advanced.GetInstance<ILanguageBranchRepository>().ListEnabled());
            typeof(EPiServer.Web.Routing.RouteCollectionExtensions).GetMethod("RegisterRoutes", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { RouteTable.Routes });
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}
