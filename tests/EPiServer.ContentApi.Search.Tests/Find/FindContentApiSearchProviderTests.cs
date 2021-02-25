using System;
using System.Collections.Generic;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.Search.Internal;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.ClientConventions;
using EPiServer.Security;
using Moq;
using Xunit;
using System.Web.OData.Query;

namespace EPiServer.ContentApi.Search.Tests.Find
{
    public class FindContentApiSearchProviderTests
    {
        private readonly ContentApiOptions _contentApiOptions;
        private readonly ContentApiConfiguration _apiConfig;
        private readonly Mock<FindContentApiSearchProvider> provider;
        private readonly ContentApiSearchConfiguration _searchConfig;
        protected Mock<ContentConvertingService> _mockContentConvertingService;

        public FindContentApiSearchProviderTests()
        {
            _contentApiOptions = new ContentApiOptions();
            _searchConfig = new ContentApiSearchConfiguration();
            _searchConfig.Default().SetMaximumSearchResults(10);

            _apiConfig = new ContentApiConfiguration();
            _contentApiOptions = _apiConfig.Default();
            _mockContentConvertingService = new Mock<ContentConvertingService>();


            provider = CreateMockProvider();
        }

        [Fact]
        public void Search_ShouldAttachQuery_WhenSetInRequest()
        {
            string query = "test query!";

            var result = provider.Object.Search(new SearchRequest(_searchConfig)
            {
                Query = query
            }, new List<string>());

            provider.Verify(x => x.AttachQuery(It.IsAny<ITypeSearch<IContent>>(), It.Is<string>(s => s == query)), Times.Once);
        }

        [Fact]
        public void Search_ShouldNotAttachQuery_WhenNull()
        {
            string query = null;

            var result = provider.Object.Search(new SearchRequest(_searchConfig)
            {
                Query = query
            }, new List<string>());

            provider.Verify(x => x.AttachQuery(It.IsAny<ITypeSearch<IContent>>(), It.Is<string>(s => s == query)), Times.Never);
        }

        [Fact]
        public void Search_ShouldNotAttachQuery_WhenEmpty()
        {
            string query = null;

            var result = provider.Object.Search(new SearchRequest(_searchConfig)
            {
                Query = query
            }, new List<string>());

            provider.Verify(x => x.AttachQuery(It.IsAny<ITypeSearch<IContent>>(), It.Is<string>(s => s == query)), Times.Never);
        }

        [Fact]
        public void Search_ShouldAttachSkip_WhenSetInRequest()
        {
            int skip = 10;

            var result = provider.Object.Search(new SearchRequest(_searchConfig)
            {
                Skip = skip
            }, new List<string>());

            provider.Verify(x => x.AttachSkip(It.IsAny<ITypeSearch<IContent>>(), It.Is<int>(s => s == skip)), Times.Once);
        }

        [Fact]
        public void Search_ShouldAttachTop_WhenSetInRequest()
        {
            int top = 13;
      
            var result = provider.Object.Search(new SearchRequest(_searchConfig)
            {
                Top = top
            }, new List<string>());

            provider.Verify(x => x.AttachTop(It.IsAny<ITypeSearch<IContent>>(), It.Is<int>(s => s == top)), Times.Once);
        }

        [Fact]
        public void Search_ShouldAlwaysAttachPublishedFilter()
        {
            var result = provider.Object.Search(new SearchRequest(_searchConfig), new List<string>());

            provider.Verify(x => x.AttachPublishedFilter(It.IsAny<ITypeSearch<IContent>>()), Times.Once);
        }

        [Fact]
        public void Search_ShouldAlwaysAttachReadAccessFilter()
        {
            var result = provider.Object.Search(new SearchRequest(_searchConfig), new List<string>());

            provider.Verify(x => x.AttachReadAccessFilter(It.IsAny<ITypeSearch<IContent>>()), Times.Once);
        }

        [Fact]
        public void Search_ShouldAttachLanguages_WhenSetInRequest()
        {
            var languages = new List<string> { "en-US", "se" };
            var result = provider.Object.Search(new SearchRequest(_searchConfig), languages);

            provider.Verify(x => x.AttachLanguageFilters(It.IsAny<ITypeSearch<IContent>>(), It.Is<List<string>>(list => list == languages)), Times.Once);
        }

        [Fact]
        public void Search_ShouldNotAttachLanguages_WhenSetInRequest()
        {
            var languages = new List<string>();
            var result = provider.Object.Search(new SearchRequest(_searchConfig), languages);

            provider.Verify(x => x.AttachLanguageFilters(It.IsAny<ITypeSearch<IContent>>(), It.Is<List<string>>(list => list == languages)), Times.Never);
        }

        [Fact]
        public void Search_ShouldAttachCaching_WhenSetInOptions()
        {
            var cacheDuration = TimeSpan.FromMinutes(2);
            _searchConfig.Default().SetSearchCacheDuration(cacheDuration);

            var languages = new List<string> { "en-US", "se" };
            var result = provider.Object.Search(new SearchRequest(_searchConfig), languages);

            provider.Verify(x => x.AttachResponseCaching(It.IsAny<ITypeSearch<IContent>>(), It.Is<TimeSpan>(time => time == cacheDuration)), Times.Once);
        }

        [Fact]
        public void Search_ShouldNotAttachCaching_WhenSetToZeroInOptions()
        {
            _searchConfig.Default().SetSearchCacheDuration(TimeSpan.Zero);

            var languages = new List<string> { "en-US", "se" };
            var result = provider.Object.Search(new SearchRequest(_searchConfig), languages);

            provider.Verify(x => x.AttachResponseCaching(It.IsAny<ITypeSearch<IContent>>(), It.IsAny<TimeSpan>()), Times.Never);
        }

        [Fact]
        public void Search_ShouldNotAttachCaching_WhenSetToNegativeInOptions()
        {
            _searchConfig.Default().SetSearchCacheDuration(TimeSpan.FromMinutes(-1));

            var languages = new List<string> { "en-US", "se" };
            var result = provider.Object.Search(new SearchRequest(_searchConfig), languages);

            provider.Verify(x => x.AttachResponseCaching(It.IsAny<ITypeSearch<IContent>>(), It.IsAny<TimeSpan>()), Times.Never);
        }

        [Fact]
        public void Search_ShouldAttachOrderBy_WhenSetInRequest()
        {
            var orderBy = "Name";
            var languages = new List<string> { "en-US", "se" };

            var result = provider.Object.Search(new SearchRequest(_searchConfig)
            {
                OrderBy = orderBy
            }, languages);

            provider.Verify(x => x.AttachOrderBy(It.IsAny<ITypeSearch<IContent>>(), It.Is<string>(s => s == orderBy)), Times.Once);
        }

        [Fact]
        public void Search_ShouldLoadFromIndex_WhenPersonalizeIsFalse()
        {
            var languages = new List<string> { "en-US", "se" };

            var result = provider.Object.Search(new SearchRequest(_searchConfig)
            {
                Personalize = false
            }, languages);

            provider.Verify(x => x.ReturnResultsViaIndex(It.IsAny<ITypeSearch<IContent>>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Search_ShouldLoadFromDatabase_WhenPersonalizeIsTrue()
        {
            var languages = new List<string> { "en-US", "se" };

            var result = provider.Object.Search(new SearchRequest(_searchConfig)
            {
                Personalize = true
            }, languages);

            provider.Verify(x => x.ReturnResultsViaDatabase(It.IsAny<ITypeSearch<IContent>>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Search_ShouldAttachFilters_WhenSetInRequest()
        {
            string filter = "ContentLink.Id eq 'Test'";
            var languages = new List<string> { "en-US", "se" };

            var result = provider.Object.Search(new SearchRequest(_searchConfig)
            {
                Filter = filter
            }, languages);

            provider.Verify(x => x.AttachFilters(It.IsAny<ITypeSearch<IContent>>(), It.Is<string>(s => s == filter)), Times.Once);
        }


        private Mock<FindContentApiSearchProvider> CreateMockProvider()
        {
            var client = new Mock<IClient>();
            var mockSearch = new Mock<ITypeSearch<IContent>>();
            mockSearch.Setup(x => x.Client).Returns(client.Object);

            _mockContentConvertingService.Setup(factory => factory.ConvertToContentApiModel(It.IsAny<IContent>(), It.IsAny<ConverterContext>())).Returns(new ContentApiModel());

            client.Setup(x => x.Search<IContent>()).Returns(mockSearch.Object);
            client.Setup(x => x.Conventions).Returns(new DefaultConventions(client.Object));

            var provider = new Mock<FindContentApiSearchProvider>(
                client.Object,
                _mockContentConvertingService.Object,
                _searchConfig,
                _apiConfig,
                new FindODataParser(client.Object, new ODataValidationSettings
                {
                    AllowedFunctions = AllowedFunctions.ToLower | AllowedFunctions.Contains | AllowedFunctions.Any,
                    AllowedLogicalOperators = AllowedLogicalOperators.LessThanOrEqual | AllowedLogicalOperators.LessThan |
                    AllowedLogicalOperators.GreaterThan | AllowedLogicalOperators.GreaterThanOrEqual |
                    AllowedLogicalOperators.Equal | AllowedLogicalOperators.NotEqual |
                    AllowedLogicalOperators.And | AllowedLogicalOperators.Or,
                    AllowedArithmeticOperators = AllowedArithmeticOperators.None,
                    AllowedQueryOptions = AllowedQueryOptions.Filter,
                })
                , new Mock<RoleService>(new Mock<IVirtualRoleRepository>().Object).Object);

            provider.Setup(x => x.ReturnResultsViaIndex(It.IsAny<ITypeSearch<IContent>>(), It.IsAny<string>()))
                .Returns(new SearchResponse());
            provider.Setup(x => x.ReturnResultsViaDatabase(It.IsAny<ITypeSearch<IContent>>(), It.IsAny<string>()))
                .Returns(new SearchResponse());



            return provider;
        }
    }
}