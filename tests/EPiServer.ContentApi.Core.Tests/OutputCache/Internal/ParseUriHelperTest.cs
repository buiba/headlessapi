using EPiServer.ContentApi.Core.OutputCache.Internal;
using EPiServer.Core;
using EPiServer.Web;
using Moq;
using System;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.OutputCache.Internal
{
    public class ParseUriHelperTest
    {
        private const string V2Uri = "http://site.com/api/episerver/v2.0/content";
        private readonly ContentReference RequestContentLink = new ContentReference(1);
        private readonly Guid RequestContentGuid = System.Guid.NewGuid();

        private readonly System.Uri GetChildrenByContentLinkUri = null;
        private readonly System.Uri GetChildrenByContentGuidUri = null;

        private readonly System.Uri GetAncestorsByContentLinkUri = null;
        private readonly System.Uri GetAncestorsByContentGuidUri = null;

        private readonly Mock<IPermanentLinkMapper> _permanentLinkMapper;

        public ParseUriHelperTest()
        {
            _permanentLinkMapper = new Mock<IPermanentLinkMapper>();

            GetChildrenByContentLinkUri = new Uri(V2Uri + $"/{RequestContentLink}/children");
            GetChildrenByContentGuidUri = new Uri(V2Uri + $"/{RequestContentGuid}/children");

            GetAncestorsByContentLinkUri = new Uri(V2Uri + $"/{RequestContentLink}/ancestors");
            GetAncestorsByContentGuidUri = new Uri(V2Uri + $"/{RequestContentGuid}/ancestors");

            _permanentLinkMapper.Setup(p => p.Find(RequestContentGuid)).Returns(new PermanentLinkMap(RequestContentGuid, RequestContentLink));
        }

        [Fact]
        public void TryParseContentLinkForRequest_IfUriIsNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ParseUriHelper.TryParseContentLinkForRequest(null, _permanentLinkMapper.Object, "children", out var contentLink));
        }

        [Fact]
        public void TryParseContentLinkForRequest_IfPermanentLinkMapperIsNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ParseUriHelper.TryParseContentLinkForRequest(GetChildrenByContentLinkUri, null, "children", out var contentLink));
        }

        [Fact]
        public void TryParseContentLinkForRequest_IfSegmentNameIsNullOrEmpty_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ParseUriHelper.TryParseContentLinkForRequest(GetChildrenByContentLinkUri, _permanentLinkMapper.Object, null, out var contentLink));
            Assert.Throws<ArgumentNullException>(() => ParseUriHelper.TryParseContentLinkForRequest(GetChildrenByContentLinkUri, _permanentLinkMapper.Object, string.Empty, out var contentLink));
        }

        #region GetChildren
        [Fact]
        public void TryParseContentLinkForRequest_GetChildrenByContentLink_IfSegmentNameNotMatched_ShouldReturnFalse()
        {
            Assert.False(ParseUriHelper.TryParseContentLinkForRequest(GetChildrenByContentLinkUri, _permanentLinkMapper.Object, "ancestors", out var contentLink));
            Assert.False(ParseUriHelper.TryParseContentLinkForRequest(GetChildrenByContentLinkUri, _permanentLinkMapper.Object, "mySegment", out var contentLink2));
        }

        [Fact]
        public void TryParseContentLinkForRequest_GetChildrenByContentGuid_IfSegmentNameNotMatched_ShouldReturnFalse()
        {
            Assert.False(ParseUriHelper.TryParseContentLinkForRequest(GetChildrenByContentGuidUri, _permanentLinkMapper.Object, "ancestors", out var contentLink));
            Assert.False(ParseUriHelper.TryParseContentLinkForRequest(GetChildrenByContentGuidUri, _permanentLinkMapper.Object, "mySegment", out var contentLink2));
        }

        [Fact]
        public void TryParseContentLinkForRequest_GetChildrenByContentLink_IfInvalidContentLink_ShouldReturnFalse()
        {
            var uri = new Uri(V2Uri + $"/invalidContentLink/children");
            Assert.False(ParseUriHelper.TryParseContentLinkForRequest(uri, _permanentLinkMapper.Object, "children", out var contentLink));
        }

        [Fact]
        public void TryParseContentLinkForRequest_GetChildrenByContentLink_IfUriAndSegmentNameAreValid_ShouldReturnTrue()
        {
            ContentReference contentLink;
            Assert.True(ParseUriHelper.TryParseContentLinkForRequest(GetChildrenByContentLinkUri, _permanentLinkMapper.Object, "children", out contentLink));
            Assert.NotNull(contentLink);
        }

        [Fact]
        public void TryParseContentLinkForRequest_GetChildrenByContentGuid_IfUriAndSegmentNameAreValid_ButNotAbleToFindPermanentLinkMap_ShouldReturnFalse()
        {
            PermanentLinkMap permanentLink = null;
            ContentReference contentLink;
            _permanentLinkMapper.Setup(p => p.Find(RequestContentGuid)).Returns(permanentLink);

            Assert.False(ParseUriHelper.TryParseContentLinkForRequest(GetChildrenByContentGuidUri, _permanentLinkMapper.Object, "children", out contentLink));
            Assert.Null(contentLink);
        }

        [Fact]
        public void TryParseContentLinkForRequest_GetChildrenByContentGuid_IfUriAndSegmentNameAreValid_AndAbleToFindPermanentLinkMap_ShouldReturnTrue()
        {
            ContentReference contentLink;
            _permanentLinkMapper.Setup(p => p.Find(RequestContentGuid)).Returns(new PermanentLinkMap(RequestContentGuid, RequestContentLink));

            Assert.True(ParseUriHelper.TryParseContentLinkForRequest(GetChildrenByContentLinkUri, _permanentLinkMapper.Object, "children", out contentLink));
            Assert.NotNull(contentLink);
        }
        #endregion

        #region GetAncestors
        [Fact]
        public void TryParseContentLinkForRequest_GetAncestorsByContentLink_IfSegmentNameNotMatched_ShouldReturnFalse()
        {
            Assert.False(ParseUriHelper.TryParseContentLinkForRequest(GetAncestorsByContentLinkUri, _permanentLinkMapper.Object, "children", out var contentLink));
            Assert.False(ParseUriHelper.TryParseContentLinkForRequest(GetAncestorsByContentLinkUri, _permanentLinkMapper.Object, "mySegment", out var contentLink2));
        }

        [Fact]
        public void TryParseContentLinkForRequest_GetAncestorsByContentGuid_IfSegmentNameNotMatched_ShouldReturnFalse()
        {
            Assert.False(ParseUriHelper.TryParseContentLinkForRequest(GetAncestorsByContentGuidUri, _permanentLinkMapper.Object, "children", out var contentLink));
            Assert.False(ParseUriHelper.TryParseContentLinkForRequest(GetAncestorsByContentGuidUri, _permanentLinkMapper.Object, "mySegment", out var contentLink2));
        }

        [Fact]
        public void TryParseContentLinkForRequest_GetAncestorsByContentLink_IfInvalidContentLink_ShouldReturnFalse()
        {
            var uri = new Uri(V2Uri + $"/invalidContentLink/ancestors");
            Assert.False(ParseUriHelper.TryParseContentLinkForRequest(uri, _permanentLinkMapper.Object, "ancestors", out var contentLink));
        }

        [Fact]
        public void TryParseContentLinkForRequest_GetAncestorsByContentLink_IfUriAndSegmentNameAreValid_ShouldReturnTrue()
        {
            ContentReference contentLink;
            Assert.True(ParseUriHelper.TryParseContentLinkForRequest(GetAncestorsByContentLinkUri, _permanentLinkMapper.Object, "ancestors", out contentLink));
            Assert.NotNull(contentLink);
        }

        [Fact]
        public void TryParseContentLinkForRequest_GetAncestorsByContentGuid_IfUriAndSegmentNameAreValid_ButNotAbleToFindPermanentLinkMap_ShouldReturnFalse()
        {
            PermanentLinkMap permanentLink = null;
            ContentReference contentLink;
            _permanentLinkMapper.Setup(p => p.Find(RequestContentGuid)).Returns(permanentLink);

            Assert.False(ParseUriHelper.TryParseContentLinkForRequest(GetAncestorsByContentGuidUri, _permanentLinkMapper.Object, "ancestors", out contentLink));
            Assert.Null(contentLink);
        }

        [Fact]
        public void TryParseContentLinkForRequest_GetAncestorsByContentGuid_IfUriAndSegmentNameAreValid_AndAbleToFindPermanentLinkMap_ShouldReturnTrue()
        {
            ContentReference contentLink;
            _permanentLinkMapper.Setup(p => p.Find(RequestContentGuid)).Returns(new PermanentLinkMap(RequestContentGuid, RequestContentLink));

            Assert.True(ParseUriHelper.TryParseContentLinkForRequest(GetAncestorsByContentGuidUri, _permanentLinkMapper.Object, "ancestors", out contentLink));
            Assert.NotNull(contentLink);
        }
        #endregion
    }
}