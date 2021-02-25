using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.Framework.Modules;
using EPiServer.Web;
using Moq;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Internal
{
    public class ContextModeResolverTest : TestBase
    {
        private new Mock<IModuleResourceResolver> _moduleResourceResolver;
        private new ContentApiConfiguration _apiConfig;

        public ContextModeResolverTest()
        {
            _moduleResourceResolver = new Mock<IModuleResourceResolver>();
            _moduleResourceResolver.Setup(m => m.ResolvePath("CMS", null)).Returns("/episerver/cms");

            _apiConfig = new ContentApiConfiguration();
            _apiConfig.Default().SetEnablePreviewMode(true);
       }

        [Fact]
        public void Resolve_WhenUrlDoesentContainCmsPath_ShouldReturnDefaultContextMode()
        {
            // Arrange
            var resolver = new ContextModeResolver(_moduleResourceResolver.Object);
            var expectedContextMode = ContextMode.Default;

            // Act
            var resolvedContextMode = resolver.Resolve("http://www.abc.com/", Web.ContextMode.Default);

            // Assert
            Assert.Equal(expectedContextMode, resolvedContextMode);
        }

        [Fact]
        public void Resolve_WhenEdit_ShouldReturnEditMode()
        {
            // Arrange
            var resolver = new ContextModeResolver(_moduleResourceResolver.Object);
            var expectedContextMode = ContextMode.Edit;

            // Act
            var resolvedContextMode = resolver.Resolve("http://www.abc.com/episerver/cms,,123?epieditmode=True", Web.ContextMode.Default);

            // Assert
            Assert.Equal(expectedContextMode, resolvedContextMode);
        }

        [Fact]
        public void Resolve_WhenPreview_ShouldReturnPreviewMode()
        {
            // Arrange
            var resolver = new ContextModeResolver(_moduleResourceResolver.Object);
            var expectedContextMode = ContextMode.Preview;

            // Act
            var resolvedContextMode = resolver.Resolve("http://www.abc.com/episerver/cms,,123", Web.ContextMode.Default);

            // Assert
            Assert.Equal(expectedContextMode, resolvedContextMode);
        }
    }
}
