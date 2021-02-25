using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using EPiServer.ContentApi.Core.Internal;
using Moq;
using Xunit;

namespace EPiServer.ContentApi.Tests.Localization
{
    public class AcceptLanguageHeaderValueProviderTests
    {

        [Fact]
        public void GetValue_ShouldReturnNull_WhenHeaderIsEmpty()
        {
            var mockActionContext = CreateMockActionContext("");

            var valueProvider = new AcceptLanguageHeaderValueProvider(mockActionContext.Request.Headers);

            var result = valueProvider.GetValue("mockString");

            Assert.Null(result);
        }

        [Fact]
        public void GetValue_ShouldReturnNullWhenHeaderIsNull()
        {
            var mockActionContext = CreateMockActionContext(null);

            var valueProvider = new AcceptLanguageHeaderValueProvider(mockActionContext.Request.Headers);

            var result = valueProvider.GetValue("mockString");

            Assert.Null(result);
        }


        [Fact]
        public void GetValue_ShouldReturnEmpty_WhenHeaderIsWildcard()
        {
            var mockActionContext = CreateMockActionContext("*");

            var valueProvider = new AcceptLanguageHeaderValueProvider(mockActionContext.Request.Headers);

            var result = valueProvider.GetValue("mockString").RawValue as IEnumerable<string>;

            Assert.Empty(result);
        }

        [Fact]
        public void GetValue_ShouldReturnLanguage_WhenHeaderIsSet()
        {
            var mockActionContext = CreateMockActionContext("en-US");

            var valueProvider = new AcceptLanguageHeaderValueProvider(mockActionContext.Request.Headers);

            var result = valueProvider.GetValue("mockString").RawValue as IEnumerable<string>;

            Assert.True(result.Count() == 1 && result.Contains("en-US"));
        }

        [Fact]
        public void GetValue_ShouldReturnMultipleLanguages_WithoutQualityWeighting()
        {
            string[] languages = {"fr-CH", "fr", "en", "de"};
            var mockActionContext = CreateMockActionContext("fr-CH, fr;q=0.9, en;q=0.8, de;q=0.7");

            var valueProvider = new AcceptLanguageHeaderValueProvider(mockActionContext.Request.Headers);

            var result = valueProvider.GetValue("mockString").RawValue as IEnumerable<string>;


            Assert.Equal(result, languages);
        }


        public HttpActionContext CreateMockActionContext(string acceptLanguageHeader)
        {
            var httpActionDescriptorMock = new Mock<HttpActionDescriptor>();

            HttpActionContext mockActionContext = new HttpActionContext()
            {
                ControllerContext = new HttpControllerContext()
                {
                    Request = new HttpRequestMessage(),
                    RequestContext = new HttpRequestContext()
                },
                ActionArguments = { { "SomeArgument", "null" } },

            };

            mockActionContext.Request.Headers.Add("Accept-Language", acceptLanguageHeader);
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
