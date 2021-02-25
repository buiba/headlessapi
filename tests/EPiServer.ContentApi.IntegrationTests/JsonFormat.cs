using System;
using System.Globalization;
using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using Newtonsoft.Json.Linq;
using Xunit;

namespace EPiServer.ContentApi.IntegrationTests
{
    [Collection(IntegrationTestCollection.Name)]
    public class JsonFormat
    {
        private const string V2Uri = "api/episerver/v2.0/content";

        private readonly ServiceFixture _fixture;

        public JsonFormat(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Get_ShouldReturnExpectedFormat()
        {
            var pageName = "validatejsonpage";
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, init: p =>
            {
                p.Name = pageName;
            });
            var startPage = _fixture.ContentRepository.Get<IContent>(ContentReference.StartPage);
            var standardPageType = ServiceLocator.Current.GetInstance<IContentTypeRepository>().Load<StandardPage>();
            await _fixture.WithContent(page, async () =>
            {
                using (new OptionsScope(false))
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                    AssertResponse.OK(contentResponse);
                    //Load up as JObject and call ToString to get nicer format
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    var date = content["changed"].ToString(Newtonsoft.Json.Formatting.None);

                    Assert.Equal(string.Format(CultureInfo.InvariantCulture, JsonTemplate, page.ContentLink.ID, page.ContentGuid, pageName, startPage.ContentLink.ID, startPage.ContentGuid, date), content.ToString());
                }
            });
        }

        [Fact]
        public async Task Get_WhenUsingOptimizedOptions_ShouldReturnExpectedOptimizedFormat()
        {
            var pageName = "validatejsonpage";
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, init: p =>
            {
                p.Name = pageName;
            });
            var startPage = _fixture.ContentRepository.Get<IContent>(ContentReference.StartPage);
            var standardPageType = ServiceLocator.Current.GetInstance<IContentTypeRepository>().Load<StandardPage>();
            using (new OptionsScope(true))
            {
                await _fixture.WithContent(page, async () =>
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}");
                    AssertResponse.OK(contentResponse);
                    //Load up as JObject and call ToString to get nicer format
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    var date = content["changed"].ToString(Newtonsoft.Json.Formatting.None);
                    Assert.Equal(string.Format(CultureInfo.InvariantCulture, JsonTemplateOptimized, page.ContentGuid, pageName, startPage.ContentGuid, date), content.ToString());
                });
            }
        }


        [Fact]
        public async Task Get_WhenSelectSpecificProperties_ShouldReturnExpectedMinifiedFormat()
        {
            var pageName = "validatejsonpage";
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, init: p =>
            {
                p.Name = pageName;
            });
            var startPage = _fixture.ContentRepository.Get<IContent>(ContentReference.StartPage);
            var standardPageType = ServiceLocator.Current.GetInstance<IContentTypeRepository>().Load<StandardPage>();

            using (new OptionsScope(true))
            {
                await _fixture.WithContent(page, async () =>
                {
                    var contentResponse = await _fixture.Client.GetAsync(V2Uri + $"/{page.ContentGuid}?select=name");
                    AssertResponse.OK(contentResponse);
                    //Load up as JObject and call ToString to get nicer format
                    var content = JObject.Parse(await contentResponse.Content.ReadAsStringAsync());
                    Assert.Equal(string.Format(CultureInfo.InvariantCulture, JsonMinifiedTemplate, page.ContentGuid, pageName), content.ToString());
                });
            }
        }

        // 0: content id
        // 1: content guid
        // 2: url segment 
        // 3: parent id
        // 4: parent guid
        // 5: shared datetime
        private static readonly string JsonTemplate = @"{{
  ""contentLink"": {{
    ""id"": {0},
    ""workId"": 0,
    ""guidValue"": ""{1}"",
    ""providerName"": null,
    ""url"": ""http://localhost/en/{2}/"",
    ""expanded"": null
  }},
  ""name"": ""{2}"",
  ""language"": {{
    ""link"": ""http://localhost/en/{2}/"",
    ""displayName"": ""English"",
    ""name"": ""en""
  }},
  ""existingLanguages"": [
    {{
      ""link"": ""http://localhost/en/{2}/"",
      ""displayName"": ""English"",
      ""name"": ""en""
    }}
  ],
  ""masterLanguage"": {{
    ""link"": ""http://localhost/en/{2}/"",
    ""displayName"": ""English"",
    ""name"": ""en""
  }},
  ""contentType"": [
    ""Page"",
    ""StandardPage""
  ],
  ""parentLink"": {{
    ""id"": {3},
    ""workId"": 0,
    ""guidValue"": ""{4}"",
    ""providerName"": null,
    ""url"": ""http://localhost/en/"",
    ""expanded"": null
  }},
  ""routeSegment"": ""{2}"",
  ""url"": ""http://localhost/en/{2}/"",
  ""changed"": {5},
  ""created"": {5},
  ""startPublish"": {5},
  ""stopPublish"": null,
  ""saved"": {5},
  ""status"": ""Published"",
  ""category"": {{
    ""value"": [],
    ""propertyDataType"": ""PropertyCategory""
  }},
  ""heading"": {{
    ""value"": """",
    ""propertyDataType"": ""PropertyLongString""
  }},
  ""mainBody"": {{
    ""value"": """",
    ""propertyDataType"": ""PropertyXhtmlString""
  }},
  ""mainContentArea"": {{
    ""value"": null,
    ""propertyDataType"": ""PropertyContentArea""
  }},
  ""targetReference"": {{
    ""value"": null,
    ""propertyDataType"": ""PropertyContentReference""
  }},
  ""contentReferenceList"": {{
    ""value"": null,
    ""propertyDataType"": ""PropertyContentReferenceList""
  }},
  ""links"": {{
    ""value"": [],
    ""propertyDataType"": ""PropertyLinkCollection""
  }},
  ""uri"": {{
    ""value"": null,
    ""propertyDataType"": ""PropertyUrl""
  }}
}}".Replace("\r\n", "\n").Replace("\n", Environment.NewLine); //It seems some build server has \n as newline while others has \r\n, so we ensure format


        // 0: content guid
        // 1: content name
        // 2: parent guid
        // 3: shared datetime
        // 4: content type guid
        // 5: site id
        private static readonly string JsonTemplateOptimized = @"{{
  ""contentLink"": {{
    ""guidValue"": ""{0}"",
    ""url"": ""http://localhost/en/{1}/"",
    ""language"": {{
      ""link"": ""http://localhost/en/{1}/"",
      ""displayName"": ""English"",
      ""name"": ""en""
    }}
  }},
  ""name"": ""{1}"",
  ""language"": {{
    ""link"": ""http://localhost/en/{1}/"",
    ""displayName"": ""English"",
    ""name"": ""en""
  }},
  ""existingLanguages"": [
    {{
      ""link"": ""http://localhost/en/{1}/"",
      ""displayName"": ""English"",
      ""name"": ""en""
    }}
  ],
  ""contentType"": [
    ""Page"",
    ""StandardPage""
  ],
  ""parentLink"": {{
    ""guidValue"": ""{2}"",
    ""url"": ""http://localhost/en/""
  }},
  ""routeSegment"": ""{1}"",
  ""url"": ""http://localhost/en/{1}/"",
  ""changed"": {3},
  ""created"": {3},
  ""startPublish"": {3},
  ""saved"": {3},
  ""status"": ""Published"",
  ""category"": [],
  ""links"": []
}}".Replace("\r\n", "\n").Replace("\n", Environment.NewLine);

        private static readonly string JsonMinifiedTemplate = @"{{
  ""contentLink"": {{
    ""guidValue"": ""{0}"",
    ""url"": ""http://localhost/en/{1}/"",
    ""language"": {{
      ""link"": ""http://localhost/en/{1}/"",
      ""displayName"": ""English"",
      ""name"": ""en""
    }}
  }},
  ""name"": ""{1}"",
  ""language"": {{
    ""link"": ""http://localhost/en/{1}/"",
    ""displayName"": ""English"",
    ""name"": ""en""
  }},
  ""contentType"": [
    ""Page"",
    ""StandardPage""
  ]
}}".Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
    }
}
