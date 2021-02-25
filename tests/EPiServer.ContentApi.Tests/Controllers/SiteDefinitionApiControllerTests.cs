using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using EPiServer.ContentApi.Cms.Controllers;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.ContentResult;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.Core.Security.Internal;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.Core.Tracking;
using EPiServer.Core;
using EPiServer.Web;
using Moq;
using Xunit;

namespace EPiServer.ContentApi.Tests.Controllers
{
    public class SiteDefinitionApiControllerTests
    {
        private readonly Mock<ISiteDefinitionConverter> _siteDefinitionConverter;
        private readonly Mock<ISiteDefinitionRepository> _siteDefinitionRepository;
        private readonly ContentApiConfiguration _apiConfig;
        private readonly SiteDefinition _currentSite;
        private readonly SiteDefinitionApiController controller;

        public SiteDefinitionApiControllerTests()
        {
            _apiConfig = new ContentApiConfiguration();

            _currentSite = new SiteDefinition() { Id = Guid.NewGuid() };

            _siteDefinitionRepository = new Mock<ISiteDefinitionRepository>();
            _siteDefinitionRepository.Setup(x => x.List()).Returns(new[] { _currentSite });

            _siteDefinitionConverter = new Mock<ISiteDefinitionConverter>();
            _siteDefinitionConverter.Setup(x => x.Convert(It.IsAny<SiteDefinition>(), It.IsAny<ConverterContext>()))
                .Returns<SiteDefinition, ConverterContext>((s, _) => new SiteDefinitionModel { Id = s.Id });

            var contextAccessor = new Mock<IContentApiTrackingContextAccessor>();
            ContentApiTrackingContext context = new ContentApiTrackingContext();
            contextAccessor.Setup(accessor => accessor.Current).Returns(context);

            controller = new SiteDefinitionApiController(
                _siteDefinitionRepository.Object,
                () => _currentSite,
                _apiConfig,
                _siteDefinitionConverter.Object,
                 contextAccessor.Object,
                 Mock.Of<ContentApiSerializerResolver>())
            {
                Request = new HttpRequestMessage(HttpMethod.Get, new Uri($"http://localhost/{RouteConstants.VersionTwoApiRoute}"))
            };
        }

        [Fact]
        public void Controller_ShouldHaveAuthorizationAttribute()
        {
            var attribute = Attribute.GetCustomAttribute(typeof(SiteDefinitionApiController),
                typeof(ContentApiAuthorizationAttribute));

            Assert.NotNull(attribute);
        }

        [Fact]
        public void Controller_ShouldHaveCorsAttribute()
        {
            var attribute = Attribute.GetCustomAttribute(typeof(SiteDefinitionApiController),
                typeof(ContentApiCorsAttribute));

            Assert.NotNull(attribute);
        }

        [Fact]
        public void Controller_ShouldHaveCorsOptionsFilter()
        {
            var attribute = Attribute.GetCustomAttribute(typeof(SiteDefinitionApiController),
                typeof(CorsOptionsActionFilter));

            Assert.NotNull(attribute);
        }

        [Fact]
        public void Get_ShouldReturnContentApiResultSiteDefintion_WithValidRequest()
        {
            _apiConfig.Default().SetMultiSiteFilteringEnabled(false);

            var result = controller.Get() as ContentApiResult<IEnumerable<SiteDefinitionModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
        }

        [Fact]
        public void Get_ShouldReturnContentApiResultSiteDefintion_WithValidRequestAndMultiSiteFiltering()
        {
            _apiConfig.Default().SetMultiSiteFilteringEnabled(true);

            var result = controller.Get() as ContentApiResult<IEnumerable<SiteDefinitionModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
        }

        [Fact]
        public void Get_WhenConverterThrowsContentNotFoundException_ShouldReturnErrorResultWithNotFoundStatus()
        {
            _siteDefinitionConverter.Setup(x => x.Convert(It.IsAny<SiteDefinition>(), It.IsAny<ConverterContext>()))
                .Throws<ContentNotFoundException>();
            _apiConfig.Default().SetMultiSiteFilteringEnabled(false);

            var result = controller.Get() as ContentApiResult<ErrorResponse>;

            Assert.Equal(HttpStatusCode.NotFound, Assert.IsAssignableFrom<ContentApiResult<ErrorResponse>>(result).StatusCode);
        }

        [Fact]
        public void Get_WhenConverterThrowsUnknownError_ShouldReturnErrorResultWithInternalServerError()
        {
            _siteDefinitionConverter.Setup(x => x.Convert(It.IsAny<SiteDefinition>(), It.IsAny<ConverterContext>()))
                .Throws<NotImplementedException>();
            _apiConfig.Default().SetMultiSiteFilteringEnabled(false);
            var result = controller.Get() as ContentApiResult<ErrorResponse>;

            Assert.Equal(HttpStatusCode.InternalServerError, Assert.IsAssignableFrom<ContentApiResult<ErrorResponse>>(result).StatusCode);
        }

        [Fact]
        public void Get_ShouldReturnContentApiResultWithSingleSiteDefintion_WithValidRequest()
        {
            _siteDefinitionRepository.Setup(x => x.List()).Returns(new[] { new SiteDefinition() { Id = Guid.NewGuid() }, _currentSite });

            _apiConfig.Default().SetMultiSiteFilteringEnabled(true);
            var result = controller.Get() as ContentApiResult<IEnumerable<SiteDefinitionModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Assert.Equal(_currentSite.Id, Assert.Single(result.Value).Id);
        }

        [Fact]
        public void Get_ShouldReturnContentApiResultWithMultipleSiteDefintions_WithValidRequest()
        {
            var sites = new[]
            {
                new SiteDefinition { Id = Guid.NewGuid() },
                new SiteDefinition { Id = Guid.NewGuid() },
                new SiteDefinition { Id = Guid.NewGuid() },
                _currentSite
            };
            _siteDefinitionRepository.Setup(x => x.List()).Returns(sites);

            _apiConfig.Default().SetMultiSiteFilteringEnabled(false);

            var result = controller.Get() as ContentApiResult<IEnumerable<SiteDefinitionModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Assert.Equal(sites.Select(x => x.Id), result.Value.Select(x => x.Id));
        }

        [Fact]
        public void Get_ShouldReturnConvertedSiteModel_WithValidRequest()
        {
            _apiConfig.Default().SetMultiSiteFilteringEnabled(true);

            var result = controller.Get() as ContentApiResult<IEnumerable<SiteDefinitionModel>>;

            Assert.Equal(HttpStatusCode.OK, result?.StatusCode);
            Assert.Equal(_currentSite.Id, result.Value.Single().Id);
        }

        private Mock<IContentApiTrackingContextAccessor> createContentApiTrackingContextAccesor()
        {
            var contextAccessor = new Mock<IContentApiTrackingContextAccessor>();
            ContentApiTrackingContext context = new ContentApiTrackingContext();
            contextAccessor.Setup(accessor => accessor.Current).Returns(context);
            return contextAccessor;
        }
    }
}
