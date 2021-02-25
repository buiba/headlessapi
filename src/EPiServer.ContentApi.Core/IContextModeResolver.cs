using EPiServer.Web;

namespace EPiServer.ContentApi.Core
{
    public interface IContextModeResolver
    {
        ContextMode Resolve(string contentUrl, ContextMode defaultContextMode);
    }
}
