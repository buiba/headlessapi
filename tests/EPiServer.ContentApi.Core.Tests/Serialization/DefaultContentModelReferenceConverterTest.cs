using System;
using Xunit;
using Moq;
using EPiServer.Web;
using EPiServer.Core;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.Web.Routing;

namespace EPiServer.ContentApi.Core.Tests.Serialization
{
    public class DefaultContentModelReferenceConverterTest
    {
        private readonly Mock<IPermanentLinkMapper> _linkMapper;
        private readonly Mock<UrlResolverService> _urlResolverService;
        private readonly DefaultContentModelReferenceConverter defaultContentModelReferenceConverter;

        public DefaultContentModelReferenceConverterTest()
        {
            _linkMapper = new Mock<IPermanentLinkMapper>();
            _urlResolverService = new Mock<UrlResolverService>();
            _linkMapper
                .Setup(x => x.Find(It.IsAny<ContentReference>()))
                .Returns((ContentReference contentReference) =>
                {
                    return new PermanentLinkMap(Guid.Empty, null);
                })
                .Verifiable();

            defaultContentModelReferenceConverter = new DefaultContentModelReferenceConverter(_linkMapper.Object, _urlResolverService.Object);
        }

        #region GetContentModelReference

        [Fact]
        public void GetContentModelReferenceIContentNull()
        {
            IContent testData = null;
            var result = defaultContentModelReferenceConverter.GetContentModelReference(testData);
            Assert.Null(result);
        }

        [Fact]
        public void GetContentModelReferenceIContent()
        {
            PageData pageData = new PageData();
            _urlResolverService.Setup(x => x.ResolveUrl(It.IsAny<ContentReference>(), It.IsAny<string>())).Returns("http://site/en/alloy-plan");

            var result = defaultContentModelReferenceConverter.GetContentModelReference(pageData);

            Assert.True(result.GuidValue == Guid.Empty && result.Id == 0 && result.WorkId == 0 && result.Url == "http://site/en/alloy-plan");
        }

        #endregion

        #region GetContentModelReference

        [Fact]
        public void GetContentModelReferenceContentReferenceNull()
        {
            ContentReference testData = null;
            var result = defaultContentModelReferenceConverter.GetContentModelReference(testData);
            Assert.Null(result);
        }

        [Fact]
        public void GetContentModelReferenceContentReference()
        {
            ContentReference contentReference = new ContentReference();
            contentReference.ID = Int32.MaxValue;
            contentReference.ProviderName = "ProviderName";
            var result = defaultContentModelReferenceConverter.GetContentModelReference(contentReference);

            Assert.True(Int32.MaxValue == result.Id && Guid.Empty == result.GuidValue && result.ProviderName == "ProviderName");
        }
        #endregion
    }
}
