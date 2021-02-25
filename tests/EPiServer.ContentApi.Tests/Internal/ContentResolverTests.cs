using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Routing;
using EPiServer.Core;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Moq;
using Xunit;

namespace EPiServer.ContentApi.Cms.Internal
{
    public class ContentResolverTests
    {
        [Fact]
        public void Resolve_WhenUrlDoesNotMatcheAnyContent_ShouldReturnNull()
        {
            const string url = "http://localhost/en/some/content/url/";

            var content = Mock.Of<IContent>();

            var subject = Subject();

            var result = subject.Resolve(url);

            Assert.Null(result);
        }

        [Fact]
        public void Resolve_WhenUrlMatchesContentExactly_ShouldReturnContent()
        {
            const string url = "http://localhost/en/some/content/url/";

            var content = Mock.Of<IContent>();

            var urlResolver = UrlResolver(url, content);
            var subject = Subject(urlResolver);

            var result = subject.Resolve(url);

            Assert.NotNull(result);
            Assert.Equal(content, result.Content);
            Assert.Null(result.RemainingRoute);
        }

        [Fact]
        public void Resolve_WhenRelativeUrlMatchesContentExactly_ShouldReturnContent()
        {
            const string url = "/some/content/url/";

            var content = Mock.Of<IContent>();

            var urlResolver = UrlResolver(url, content);
            var subject = Subject(urlResolver);

            var result = subject.Resolve(url);

            Assert.NotNull(result);
            Assert.Equal(content, result.Content);
            Assert.Null(result.RemainingRoute);
        }

        [Fact]
        public void Resolve_WhenStartOfUrlMatchesContentAndExactMatchIsNotRequired_ShouldReturnContentAndRemainingPath()
        {
            const string partialUrl = "http://localhost/en/partial/";
            const string additionalSegments = "additional/segments/";
            var completeUrl = partialUrl + additionalSegments;

            var content = Mock.Of<IContent>();

            var urlResolver = UrlResolver(completeUrl, content, "/" + additionalSegments);
            var subject = Subject(urlResolver);

            var result = subject.Resolve(completeUrl, matchExact: false);

            Assert.NotNull(result);
            Assert.Equal(content, result.Content);
            Assert.Equal("/" + additionalSegments, result.RemainingRoute);
        }

        [Fact]
        public void Resolve_WhenStartOfUrlMatchesContentAndExactMatchIsRequired_ShouldReturnNull()
        {
            const string url = "http://localhost/en/some/content/url/";

            var urlResolver = UrlResolver("http://localhost/en/some/content/", Mock.Of<IContent>());
            var subject = Subject(urlResolver);

            var result = subject.Resolve(url, matchExact: true);

            Assert.Null(result);
        }

        [Fact]
        public void Resolve_WhenStartOfUrlMatchesContentAndExactMatchIsNotRequired_ShouldReturnParentContent()
        {
            const string url = "http://localhost/en/parent/child/";
            const string remainingPath = "/child/";

            var dict = new Dictionary<string, IContent>
            {
                { url, Mock.Of<IContent>() },
                { "http://localhost/en/some/other/content/", Mock.Of<IContent>() }
            };

            var urlResolver = UrlResolver(dict, remainingPath);
            var subject = Subject(urlResolver);

            var result = subject.Resolve(url, matchExact: false);

            Assert.Equal(dict[url], result.Content);
            Assert.Equal(remainingPath, result.RemainingRoute);
        }

        [Fact]
        public void Resolve_WhenUrlMatchesContentExactlyAndExactMatchIsNotRequired_ShouldReturnContent()
        {
            const string url = "http://localhost/en/parent/child/";
            const string parentUrl = "http://localhost/en/parent/";

            var dict = new Dictionary<string, IContent>
            {
                { parentUrl, Mock.Of<IContent>() },
                { url, Mock.Of<IContent>() }
            };

            var urlResolver = UrlResolver(dict);
            var subject = Subject(urlResolver);

            var result = subject.Resolve(url, matchExact: false);

            Assert.Equal(dict[url], result.Content);
        }

        [Fact]
        public void Resolve_WhenUrlIsEditAndPreviewIsEnabled_ShouldReturnContent()
        {
            var url = "http://localhost/episerver/cms,,123?epieditmode=True";

            var content = Mock.Of<IContent>();

            var urlResolver = UrlResolver(url, content);
            var contextModeResolver = ContextModeResolver(url, ContextMode.Edit);
            var subject = Subject(urlResolver, contextModeResolver);

            var result = subject.Resolve(url, allowPreview: true);

            Assert.NotNull(result);
            Assert.Equal(content, result.Content);
        }

        [Fact]
        public void Resolve_WhenUrlIsEditAndPreviewIsDisabled_ShouldReturnNull()
        {
            var url = "http://localhost/episerver/cms,,123?epieditmode=True";

            var urlResolver = UrlResolver(url, Mock.Of<IContent>());
            var contextModeResolver = ContextModeResolver(url, ContextMode.Edit);
            var subject = Subject(urlResolver, contextModeResolver);

            var result = subject.Resolve(url, allowPreview: false);

            Assert.Null(result);
        }

        [Fact]
        public void Resolve_WhenUrlIsPreviewAndPreviewIsEnabled_ShouldReturnContent()
        {
            const string url = "http://localhost/this/is/just/a/mock/url/";

            var content = Mock.Of<IContent>();

            var urlResolver = UrlResolver(url, content);
            var contextModeResolver = ContextModeResolver(url, ContextMode.Preview);
            var subject = Subject(urlResolver, contextModeResolver);

            var result = subject.Resolve(url, allowPreview: true);

            Assert.NotNull(result);
            Assert.Equal(content, result.Content);
        }

        [Fact]
        public void Resolve_WhenUrlIsPreviewAndPreviewIsDisabled_ShouldReturnNull()
        {
            const string url = "http://localhost/this/is/just/a/mock/url/";

            var urlResolver = UrlResolver(url, Mock.Of<IContent>());
            var contextModeResolver = ContextModeResolver(url, ContextMode.Preview);
            var subject = Subject(urlResolver, contextModeResolver);

            var result = subject.Resolve(url, allowPreview: false);

            Assert.Null(result);
        }

        private static UrlResolver UrlResolver(string url, IContent routedContent = null, string remainningPath = null)
        {
            var mock = new Mock<UrlResolver>();
            mock.Setup(x => x.Route(It.Is<string>(s => StringComparer.Ordinal.Equals(url, s)), It.IsAny<RouteArguments>()))
                .Returns(new ContentRouteData(routedContent, null, remainningPath));

            return mock.Object;
        }

        private static UrlResolver UrlResolver(Dictionary<string, IContent> routedContent, string remainingPath = null)
        {
            var mock = new Mock<UrlResolver>();

            mock.Setup(x => x.Route(It.Is<string>(s => routedContent.ContainsKey(s)), It.IsAny<RouteArguments>()))
                .Returns<string, RouteArguments>((s, r) => new ContentRouteData(routedContent.ContainsKey(s) ? routedContent[s] : null, null, remainingPath));

            return mock.Object;
        }

        private static Core.IContextModeResolver ContextModeResolver(string url = null, ContextMode resolvedContextMode = ContextMode.Default)
        {
            var mock = new Mock<Core.IContextModeResolver>();
            if (url is null)
            {
                mock.Setup(x => x.Resolve(It.IsAny<string>(), It.IsAny<ContextMode>())).Returns(resolvedContextMode);
            }
            else
            {
                mock.Setup(x => x.Resolve(url, It.IsAny<ContextMode>())).Returns(resolvedContextMode);
            }
            return mock.Object;
        }

        private static ContentResolver Subject(UrlResolver urlResolver = null, Core.IContextModeResolver contextModeResolver = null)
            => new ContentResolver(urlResolver ?? Mock.Of<UrlResolver>(), contextModeResolver ?? ContextModeResolver());
    }

}
