using EPiServer.ContentApi.Core.Internal;
using EPiServer.Core;
using EPiServer.Web;
using EPiServer.Web.Routing;

namespace EPiServer.ContentApi.Core.Tests.Business
{
    public class ContentLoaderServiceExpose : ContentLoaderService
    {
        public ContentLoaderServiceExpose(IContentLoader contentLoader, IPermanentLinkMapper permanentLinkMapper, IUrlResolver urlResolver, ContextModeResolver contextModeResolver, IContentProviderManager providerManager)
            : base(contentLoader, permanentLinkMapper, urlResolver, contextModeResolver, providerManager)
        {
        }

        public bool ShouldContentBeExposed_Exposed(IContent content)
        {
            return base.ShouldContentBeExposed(content);
        }

        protected override LanguageSelector CreateLoaderOptions(string language, bool shouldUseMasterIfFallbackNotExist = false)
        {

           return LanguageSelector.Fallback("en", false);
        }

    }
}
