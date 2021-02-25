using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.ContentResult;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.Core;
using EPiServer.Core.Internal;
using EPiServer.Framework.Modules;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Security;
using EPiServer.Web;
using Moq;
using System.Collections.Generic;
using System.Security.Principal;

namespace EPiServer.ContentApi.Core.Tests
{
    public class TestBase
    {
        protected IServiceLocator _locator;
        protected readonly Mock<IVirtualRoleRepository> _virtualRoleRepository;
        protected readonly RoleService _contentApiRoleService;
        protected readonly Mock<UIRoleProvider> _uiRoleProvider;
        protected List<ContentApiClient> _defaultClients;        
        protected ContentApiConfiguration _apiConfig;
        protected ContentFragmentFactory _fragmentFactory;
        
        protected readonly Mock<IContentRepository> _contentRepository;
        protected readonly DisplayOptions _displayOptions;
        protected readonly Mock<IPublishedStateAssessor> _publishedStateVerifier;
        protected readonly Mock<IContentAccessEvaluator> _contentAccessEvaluator;
        protected readonly Mock<IPermanentLinkMapper> _permanentLinkMapper;
        protected readonly Mock<EPiServer.Web.IContextModeResolver> _contextModeResolver;
        protected readonly Mock<IModuleResourceResolver> _moduleResourceResolver;

        public TestBase()
        {
            _defaultClients = new List<ContentApiClient>
            {
                new ContentApiClient
                {
                    ClientId = "Default",
                    AccessControlAllowOrigin = "*"
                }
            };

            _apiConfig = new ContentApiConfiguration();
            _apiConfig.Default().SetClients(_defaultClients);
            _apiConfig.Default().SetEnablePreviewMode(true);

            _virtualRoleRepository = new Mock<IVirtualRoleRepository>();
            _contentApiRoleService = new RoleService(_virtualRoleRepository.Object);
            _uiRoleProvider = new Mock<UIRoleProvider>();

            _contentRepository = new Mock<IContentRepository>();
            _displayOptions = new DisplayOptions();

            _publishedStateVerifier = new Mock<IPublishedStateAssessor>();
            _publishedStateVerifier.Setup(x => x.IsPublished(It.IsAny<IContent>(), It.IsAny<PublishedStateCondition>())).Returns(true);

            _contentAccessEvaluator = new Mock<IContentAccessEvaluator>();
            _contentAccessEvaluator.Setup(x => x.HasAccess(It.IsAny<IContent>(), It.IsAny<IPrincipal>(), It.IsAny<AccessLevel>())).Returns(true);

            _permanentLinkMapper = new Mock<IPermanentLinkMapper>();
            _permanentLinkMapper.Setup(mapper => mapper.Find(It.IsAny<ContentReference>())).Returns((ContentReference contentRef) => {
                return new PermanentLinkMap(System.Guid.NewGuid(), new ContentReference(1));
            });
            _permanentLinkMapper.Setup(mapper => mapper.Find(It.IsAny<System.Guid>())).Returns((ContentReference contentRef) => {
                return new PermanentLinkMap(System.Guid.NewGuid(), new ContentReference(1));
            });

            _contextModeResolver = new Mock<EPiServer.Web.IContextModeResolver>();
            _contextModeResolver.SetupGet(ctm => ctm.CurrentMode).Returns(ContextMode.Default);

            _moduleResourceResolver = new Mock<IModuleResourceResolver>();
            _moduleResourceResolver.Setup(m => m.ResolvePath("CMS", null)).Returns("/episerver/cms");


            _fragmentFactory = new ContentFragmentFactory(
                _contentRepository.Object,
                _displayOptions,
                _publishedStateVerifier.Object,
                _contentAccessEvaluator.Object,
                _permanentLinkMapper.Object,
                _contextModeResolver.Object);

            SetupServiceLocator();
        }

        protected virtual void SetupServiceLocator()
        {
            var container = new StructureMap.Container();
            container.Configure(x =>
            {
                x.For<UIRoleProvider>().Use(_uiRoleProvider.Object);
                x.For<IVirtualRoleRepository>().Use(_virtualRoleRepository.Object);
                x.For<ContentApiConfiguration>().Use(_apiConfig);
                x.For<IJsonSerializerConfiguration>().Use(Mock.Of<IJsonSerializerConfiguration>());
                x.For<ContentFragmentFactory>().Use(_fragmentFactory);
                x.For<IContextModeResolver>().Use(new ContextModeResolver(_moduleResourceResolver.Object));
            });

            _locator = new TestStructureMapServiceLocator(container);
            ServiceLocator.SetLocator(_locator);
        }
    }
}
