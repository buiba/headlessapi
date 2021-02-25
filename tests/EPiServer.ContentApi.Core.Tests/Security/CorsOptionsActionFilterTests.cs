using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Security.Internal;
using EPiServer.ServiceLocation;
using Moq;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Security
{
    public class CorsOptionsActionFilterTests
    {
        [Fact]
        public void OnActionExecuting_ShouldReturnOk_WhenOptionsRequest()
        {
            var mockActionContext = CreateMockActionContext(HttpMethod.Options);

            var filter = new CorsOptionsActionFilter();

            filter.OnActionExecuting(mockActionContext);

            Assert.Equal(HttpStatusCode.OK, mockActionContext.Response.StatusCode);
        }

        [Fact]
        public void OnActionExecuting_ShouldNotCreateResponse_WhenNotOptionsRequest()
        {
            var mockActionContext = CreateMockActionContext(HttpMethod.Get);

            var filter = new CorsOptionsActionFilter();

            filter.OnActionExecuting(mockActionContext);

            Assert.Null(mockActionContext.Response);
        }

        public HttpActionContext CreateMockActionContext(HttpMethod method)
        {
            var httpActionDescriptorMock = new Mock<HttpActionDescriptor>();

            httpActionDescriptorMock
                .Setup(x => x.GetCustomAttributes<AllowAnonymousAttribute>())
                .Returns(new Collection<AllowAnonymousAttribute>());

            HttpActionContext mockActionContext = new HttpActionContext()
            {
                ControllerContext = new HttpControllerContext()
                {
                    Request = new HttpRequestMessage(method, "http://test.com"),
                    RequestContext = new HttpRequestContext()
                },
                ActionArguments = { { "SomeArgument", "null" } },

            };

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
