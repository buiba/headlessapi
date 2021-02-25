using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Forms;
using EPiServer.Forms.Configuration;
using EPiServer.Forms.Implementation.Elements;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Moq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;

namespace EPiServer.ContentApi.Forms.Tests
{
    public class TestBase
    {
        protected Mock<IContentTypeRepository> _contentTypeRepository;
        protected Mock<ReflectionService> _reflectionService;
        protected Mock<IContentModelReferenceConverter> _contentModelService;
        protected Mock<IUrlResolver> _urlResolver;
        protected Mock<UrlResolverService> _urlResolverService;
        protected Mock<FormResourceService> _formResourceService;
        protected Mock<IContentVersionRepository> _contentVersionRepository;
        protected Mock<ContentLoaderService> _contentLoaderService;
        protected Mock<IEPiServerFormsImplementationConfig> _formConfig;
        protected Mock<FormRenderingService> _formRenderingService;
        protected FormContentModelMapper _mapper;
        protected FormContainerBlock _formContainerBlock;
        protected const int _numberOfAssets = 6;

        public TestBase()
        {
            // initialize form container 
            _formContainerBlock = ProxyGenerator.OfType<FormContainerBlock>();
            (_formContainerBlock as IVersionable).Status = VersionStatus.Published;
            (_formContainerBlock as ILocalizable).Language = new CultureInfo("en-us");

            // initialize ContentModelService
            _contentModelService = new Mock<IContentModelReferenceConverter>();
            _contentModelService.Setup(svc => svc.GetContentModelReference(It.IsAny<IContent>())).Returns(new ContentModelReference());

            // initialize content loader service
            _contentLoaderService = new Mock<ContentLoaderService>();
            _contentLoaderService.Setup(loader => loader.Get(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns(new PageData());

            // initialize contentTypeRepository
            _contentTypeRepository = new Mock<IContentTypeRepository>();
            _contentTypeRepository.Setup(repo => repo.Load(It.IsAny<int>())).Returns(new ContentType() { Name = "FormContainerBlock" });

            // initialize urlResolver
            _urlResolver = new Mock<IUrlResolver>();
            _urlResolver.Setup(resolver => resolver.GetUrl(It.IsAny<ContentReference>(), It.IsAny<string>(), It.IsAny<UrlResolverArguments>()))
                        .Returns(string.Empty);

            // initialize form config
            _formConfig = new Mock<IEPiServerFormsImplementationConfig>();
            _formConfig.SetupGet(config => config.InjectFormOwnJQuery).Returns(true);
            _formConfig.SetupGet(config => config.InjectFormOwnStylesheet).Returns(true);

            _formRenderingService = new Mock<FormRenderingService>(null, null, null, null, null, null);
            _formRenderingService.Setup(x => x.GetScriptFromAssemblyByName(It.IsAny<string>())).Returns((string name) => { return "scriptContent"; });
            _formRenderingService.Setup(x => x.GetInlineAssets(It.IsAny<FormContainerBlock>(), It.IsAny<string>())).Returns((FormContainerBlock form, string name) =>
            {
                return new Dictionary<string, string>()
                    {
                        { "OriginalJquery", "scriptContent"},
                        { "Prerequisite", "scriptContent"}
                    };
            });

            //initialize urlResolverService
            _urlResolverService = new Mock<UrlResolverService>();

            _formResourceService = new Mock<FormResourceService>();

            var propertyConverter = new Mock<IPropertyConverter>();
            propertyConverter.Setup(p => p.Convert(It.IsAny<PropertyData>(), It.IsAny<ConverterContext>())).Returns(Mock.Of<IPropertyModel>());
            var propertyConverterResolver = new Mock<IPropertyConverterResolver>();
            propertyConverterResolver.Setup(p => p.Resolve(It.IsAny<PropertyData>())).Returns(propertyConverter.Object);

            // initialize contentVersionRepository
            _contentVersionRepository = new Mock<IContentVersionRepository>();
            _mapper = new FormContentModelMapper(_contentTypeRepository.Object, null, _contentModelService.Object,
                propertyConverterResolver.Object, _contentVersionRepository.Object, _contentLoaderService.Object,
                _formConfig.Object, _formRenderingService.Object, _urlResolverService.Object,
                new ContentApiConfiguration());
        }

        protected HttpContext CreateHttpContext(string requestUrl, string queryString)
        {
            var httpContext = new HttpContext(
                    new HttpRequest("", requestUrl, queryString),
                    new HttpResponse(new StringWriter())
                );

            return httpContext;
        }
    }
}
