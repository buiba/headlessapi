using System;
using System.Collections.Generic;
using System.Net;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.ContentResult;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.Core.Security.Internal;
using EPiServer.ContentApi.Search.Controllers;
using EPiServer.ContentApi.Search.Internal;
using EPiServer.ServiceLocation;
using Moq;
using Xunit;

namespace EPiServer.ContentApi.Search.Tests.Controllers
{
    public class ContentApiSearchControllerTests
    {
        private readonly Mock<IServiceLocator> _locator;
        private readonly Mock<IContentApiSearchProvider> searchProvider;
        private readonly ContentApiSearchConfiguration _searchConfig;
        private readonly ContentApiSearchController controller;

        public ContentApiSearchControllerTests()
        {
            _locator = new Mock<IServiceLocator>();
            _locator.Setup(l => l.GetInstance<ContentApiConfiguration>()).Returns(new ContentApiConfiguration());
            ServiceLocator.SetLocator(_locator.Object);

            searchProvider = new Mock<IContentApiSearchProvider>();
            searchProvider.Setup(x => x.Search(It.IsAny<SearchRequest>(), It.IsAny<List<string>>()))
                .Returns(new SearchResponse());

            _searchConfig = new ContentApiSearchConfiguration();
            _searchConfig.Default().SetMaximumSearchResults(10);

            controller = new ContentApiSearchController(searchProvider.Object, _searchConfig, new ContentApiConfiguration(), Mock.Of<ContentApiSerializerResolver>());
        }

        [Fact]
        public void Controller_ShouldHaveAuthorizationAttribute()
        {
            var attribute = Attribute.GetCustomAttribute(typeof(ContentApiSearchController),
                typeof(ContentApiAuthorizationAttribute));

            Assert.NotNull(attribute);
        }

        [Fact]
        public void Controller_ShouldHaveCorsAttribute()
        {
            var attribute = Attribute.GetCustomAttribute(typeof(ContentApiSearchController),
                typeof(ContentApiCorsAttribute));

            Assert.NotNull(attribute);
        }

        [Fact]
        public void Controller_ShouldHaveCorsOptionsFilter()
        {
            var attribute = Attribute.GetCustomAttribute(typeof(ContentApiSearchController),
                typeof(CorsOptionsActionFilter));

            Assert.NotNull(attribute);
        }

        [Fact]
        public void Search_ShouldInitilizeTheDefaultRequest_WhenRequestIsNull()
        {
            var result = controller.Search(null, new List<string>() { "en-US" }) as ContentApiResult<SearchResponse>;

            searchProvider.Verify(x => x.Search(It.Is<SearchRequest>(m => m != null), It.IsAny<List<string>>()), Times.Once);
        }

        [Fact]
        public void Search_ShouldReturnSearchResponse_WithValidRequest()
        {
            var result = controller.Search(new SearchRequest(_searchConfig), new List<string>() { "en-US" }) as ContentApiResult<SearchResponse>;

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void Search_ShouldReturnBadRequest_WhenTopIsTooLarge()
        {
          //  searchOptions.SetMaximumSearchResults(50);
            _searchConfig.Default().SetMaximumSearchResults(50);

            var result = controller.Search(new SearchRequest(_searchConfig)
            {
                Top = 80
            }, new List<string>() { "en-US" }) as ContentApiErrorResult;

          
            searchProvider.Verify(x => x.Search(It.Is<SearchRequest>(m => m.Top == 50), It.IsAny<List<string>>()), Times.Once);
        }

        [Fact]
        public void Search_ShouldReturnBadRequest_WhenTopIsLessThan1()
        {
            var result = controller.Search(new SearchRequest(_searchConfig)
            {
                Top = 0
            }, new List<string>() { "en-US" }) as ContentApiErrorResult;

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(ErrorCode.InputOutOfRange, result.Value.Error.Code);
        }

        [Fact]
        public void Search_ShouldReturnBadRequest_WhenSkipIsLessThan0()
        {
            var result = controller.Search(new SearchRequest(_searchConfig)
            {
                Skip = -1
            }, new List<string>() { "en-US" }) as ContentApiErrorResult;

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(ErrorCode.InputOutOfRange, result.Value.Error.Code);
        }

        [Fact]
        public void Search_ShouldReturnInternalServerError_WhenSearchProviderThrowsException()
        {
            searchProvider.Setup(x => x.Search(It.IsAny<SearchRequest>(), It.IsAny<List<string>>()))
                          .Throws(new Exception("test"));

            var result = controller.Search(new SearchRequest(_searchConfig), new List<string>() { "en-US" }) as ContentApiErrorResult;

            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.Equal(ErrorCode.InternalServerError, result.Value.Error.Code);
        }

        [Fact]
        public void Search_ShouldReturnBadRequest_WhenSearchProviderThrowsFilterParseException()
        {
            searchProvider.Setup(x => x.Search(It.IsAny<SearchRequest>(), It.IsAny<List<string>>()))
                          .Throws(new FilterParseException("test"));

            var result = controller.Search(new SearchRequest(_searchConfig), new List<string>() { "en-US" }) as ContentApiErrorResult;

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(ErrorCode.InvalidFilterClause, result.Value.Error.Code);
        }

        [Fact]
        public void Search_ShouldReturnBadRequest_WhenSearchProviderThrowsOrderByParseException()
        {
            searchProvider.Setup(x => x.Search(It.IsAny<SearchRequest>(), It.IsAny<List<string>>()))
                          .Throws(new OrderByParseException("test"));

            var result = controller.Search(new SearchRequest(_searchConfig), new List<string>() { "en-US" }) as ContentApiErrorResult;

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(ErrorCode.InvalidOrderByClause, result.Value.Error.Code);
        }

        [Fact]
        public void Search_ShouldCallSearchProvider_WhenRequestIsValid()
        {
            var result = controller.Search(new SearchRequest(_searchConfig), new List<string>() { "en-US" }) as ContentApiResult<SearchResponse>;

            searchProvider.Verify(x => x.Search(It.IsAny<SearchRequest>(), It.IsAny<List<string>>()), Times.Once);
        }

    }
}
