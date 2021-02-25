using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.ContentApi.Core.Tests;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Moq;
using System;
using System.Collections.Generic;
using EPiServer.ContentApi.Commerce.Internal;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Configuration;

namespace EPiServer.ContentApi.Commerce.Tests
{
    public class TestBase
    {
        private static object _lockObject = new object();

        protected Mock<IContentTypeRepository> _contentTypeRepository;
        protected readonly Mock<ContentLoaderService> _contentLoaderService;
        protected readonly IContentModelReferenceConverter _contentModelService;
        protected readonly Mock<UrlResolverService> _urlResolverService;
        protected IServiceLocator _locator;
        protected readonly Mock<IUrlResolver> _urlResolver;
        internal CommerceContentModelMapper _mapper;
        internal List<ICatalogContentModelBuilder> _builders = new List<ICatalogContentModelBuilder>();
        protected ItemCollection<CommerceMedia> _commerceMediaCollection;
        protected Mock<IContentVersionRepository> _contentVersionRepository;
        protected List<VariationContent> _variantContentList;

        public TestBase()
        {
            _urlResolver = new Mock<IUrlResolver>();
            var linkMapper = new Mock<IPermanentLinkMapper>();
            linkMapper
                .Setup(x => x.Find(It.IsAny<Guid>()))
                .Returns(() => new PermanentLinkMap(Guid.NewGuid(), ContentReference.EmptyReference));
            _urlResolverService = new Mock<UrlResolverService>();
            _urlResolverService.Setup(x => x.ResolveUrl(It.IsAny<ContentReference>(), It.IsAny<string>()))
                .Returns(String.Empty);
            _contentLoaderService = new Mock<ContentLoaderService>();
            _contentModelService = new DefaultContentModelReferenceConverter(linkMapper.Object, _urlResolverService.Object);
            SetupModelBuilders();

            // initialize contentTypeRepository
            _contentTypeRepository = new Mock<IContentTypeRepository>();
            _contentTypeRepository.Setup(repo => repo.Load(It.IsAny<int>())).Returns(new ContentType() { Name = "FormContainerBlock" });

            _commerceMediaCollection = new ItemCollection<CommerceMedia>()
                    {
                        new CommerceMedia
                        {
                            AssetLink = new ContentReference(10)
                        }
                    };

            _variantContentList = new List<VariationContent>
                    {
                        new VariationContent
                        {
                            ContentLink = new PageReference(12, 3),
                            ParentLink = new PageReference(16, 4)
                        }
                    };

            // create parent pages
            var parentProperties = new PropertyDataCollection();
            parentProperties.Add("PageLink", new PropertyPageReference(new PageReference(16, 4)));
            PageData parentPage = new PageData(new AccessControlList(), parentProperties);
            _contentVersionRepository = new Mock<IContentVersionRepository>();
            // mock get parent page
            _contentLoaderService.Setup(cl => cl.Get(It.Is<ContentReference>(x => x.ID == 16), It.IsAny<string>(), It.IsAny<bool>())).Returns(parentPage);

            _mapper = new CommerceContentModelMapper(
                _contentTypeRepository.Object,
                null,
                _contentModelService,
                Mock.Of<IPropertyConverterResolver>(),
                _contentVersionRepository.Object,
                _contentLoaderService.Object,
                _urlResolverService.Object,
                new ContentApiConfiguration(),
                _builders);
        }

        protected void SetupModelBuilders()
        {
            var bundleContentModelBuilder = new BundleContentModelBuilder(_contentLoaderService.Object, _contentModelService, _urlResolverService.Object);
            var nodeContentModelBuilder = new NodeContentModelBuilder(_contentLoaderService.Object, _contentModelService, _urlResolverService.Object);
            var packageContentModelBuilder = new PackageContentModelBuilder(_contentLoaderService.Object, _contentModelService, _urlResolverService.Object);
            var productContentModelBuilder = new ProductContentModelBuilder(_contentLoaderService.Object, _contentModelService, _urlResolverService.Object);
            var variationContentModelBuilder = new VariationContentModelBuilder(_contentLoaderService.Object, _contentModelService, _urlResolverService.Object);

            _builders.Add(bundleContentModelBuilder);
            _builders.Add(nodeContentModelBuilder);
            _builders.Add(packageContentModelBuilder);
            _builders.Add(productContentModelBuilder);
            _builders.Add(variationContentModelBuilder);

        }


    }
}
