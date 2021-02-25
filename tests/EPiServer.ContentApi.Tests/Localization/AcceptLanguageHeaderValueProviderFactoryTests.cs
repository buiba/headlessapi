using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Http.Controllers;
using EPiServer.ContentApi.Core.Internal;
using Moq;
using Xunit;

namespace EPiServer.ContentApi.Tests.Localization
{
    public class AcceptLanguageHeaderValueProviderFactoryTests
    {
        [Fact]
        public void ReturnsCorrectValueProvider()
        {
            var providerFactory = new AcceptLanguageHeaderValueProviderFactory();

            var mockActionContext = CreateMockActionContext();

            var valueProvider = providerFactory.GetValueProvider(mockActionContext);

            Assert.IsType<AcceptLanguageHeaderValueProvider>(valueProvider);
        }

        public HttpActionContext CreateMockActionContext(IPrincipal principal = null)
        {
            var httpActionDescriptorMock = new Mock<HttpActionDescriptor>();

            HttpActionContext mockActionContext = new HttpActionContext()
            {
                ControllerContext = new HttpControllerContext()
                {
                    Request = new HttpRequestMessage(),
                    RequestContext = new HttpRequestContext() { Principal = principal }
                },
                ActionArguments = { { "SomeArgument", "null" } },

            };
            
            mockActionContext.Request.Headers.Add("Accept-Language", "en-US");
            httpActionDescriptorMock.Object.ControllerDescriptor =
                new HttpControllerDescriptor();
            mockActionContext.ActionDescriptor = httpActionDescriptorMock.Object;

            var httpConfiguration = new HttpConfiguration();
            mockActionContext.ControllerContext.ControllerDescriptor = new HttpControllerDescriptor(httpConfiguration, "Test", typeof(ApiController));
            mockActionContext.ControllerContext.Configuration = httpConfiguration;
            mockActionContext.ControllerContext.Configuration.Formatters.Add(new JsonMediaTypeFormatter());
            return mockActionContext;
        }
    }
}
