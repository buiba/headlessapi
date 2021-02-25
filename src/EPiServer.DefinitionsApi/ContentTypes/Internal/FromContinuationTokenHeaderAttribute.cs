using System.Web.Http;
using System.Web.Http.Controllers;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    // This class will be completely different in .NET Core - so don't bother creating reusable component.
    internal sealed class FromContinuationTokenHeaderAttribute : ParameterBindingAttribute
    {
        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter) => new FromHeaderBinding(parameter, ContinuationToken.HeaderName);
    }
}
