using System;
using System.Globalization;
using System.Threading.Tasks;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.ContentResult;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using Newtonsoft.Json.Linq;
using Xunit;

namespace EPiServer.ContentApi.Core.IntegrationTests
{
    [Collection(IntegrationTestCollection.Name)]
    public class Serializer
    {
        private readonly ServiceFixture _fixture;
        private readonly IContentLoader _contentLoader;
        private readonly ContentConvertingService _contentConverterService;
        private readonly ContentApiConfiguration _contentApiConfiguration;
        private readonly ContentApiSerializerResolver _serializationResolver;

        public Serializer(ServiceFixture fixture)
        {
            _fixture = fixture;
            _contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            _contentConverterService = ServiceLocator.Current.GetInstance<ContentConvertingService>();
            _contentApiConfiguration = ServiceLocator.Current.GetInstance<ContentApiConfiguration>();
            _serializationResolver = ServiceLocator.Current.GetInstance<ContentApiSerializerResolver>();
        }

        [Fact]
        public async Task Serializer_ShouldReturnExpectedFormat()
        {
            var pageName = "validatejsonpage";
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, init: p =>
            {
                p.Name = pageName;
            });
            var startPage = _fixture.ContentRepository.Get<IContent>(ContentReference.StartPage);
            var standardPageType = ServiceLocator.Current.GetInstance<IContentTypeRepository>().Load<StandardPage>();
            await _fixture.WithContent(page, () =>
            {
                using (var scope = new OptionsScope(false))
                {
                    var context = new ConverterContext(scope.GetScopedApiOptions(), null, null, true, page.Language);
                    var content = _contentLoader.Get<StandardPage>(page.ContentGuid);
                    var contentModel = _contentConverterService.Convert(content, context);
                    var json = _serializationResolver.Resolve(context.Options).Serialize(contentModel);
                    var jObject = JObject.Parse(json);
                    var date = jObject["changed"].ToString(Newtonsoft.Json.Formatting.None);
                    Assert.Equal(string.Format(CultureInfo.InvariantCulture, JsonTemplate, page.ContentLink.ID, page.ContentGuid, pageName, startPage.ContentLink.ID, startPage.ContentGuid, date), jObject.ToString());
                    return Task.CompletedTask;
                }
            });
        }

        [Fact]
        public async Task Serializer_WithOptimizedOption_ShouldReturnExpectedFormat()
        {
            var pageName = "validatejsonpage";
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, init: p =>
            {
                p.Name = pageName;
            });
            var startPage = _fixture.ContentRepository.Get<IContent>(ContentReference.StartPage);
            var standardPageType = ServiceLocator.Current.GetInstance<IContentTypeRepository>().Load<StandardPage>();

            await _fixture.WithContent(page, () =>
            {
                using (new OptionsScope(true))
                {
                    var context = new ConverterContext(_contentApiConfiguration.Default(), null, null, true, page.Language);
                    var content = _contentLoader.Get<StandardPage>(page.ContentGuid);
                    var contentModel = _contentConverterService.Convert(content, context);
                    var json = _serializationResolver.Resolve(context.Options).Serialize(contentModel);
                    var jObject = JObject.Parse(json);
                    var date = jObject["changed"].ToString(Newtonsoft.Json.Formatting.None);
                    Assert.Equal(string.Format(CultureInfo.InvariantCulture, JsonTemplateOptimized, page.ContentGuid, pageName, startPage.ContentGuid, date), jObject.ToString());
                    return Task.CompletedTask;
                }
            });
        }

        [Fact]
        public async Task Serializer_WhenSelectSpecificProperties_ShouldReturnExpectedMinifiedFormat()
        {
            var pageName = "validatejsonpage";
            var page = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, init: p =>
            {
                p.Name = pageName;
            });
            var startPage = _fixture.ContentRepository.Get<IContent>(ContentReference.StartPage);
            var standardPageType = ServiceLocator.Current.GetInstance<IContentTypeRepository>().Load<StandardPage>();
            await _fixture.WithContent(page, () =>
            {
                using (new OptionsScope(true))
                {
                    var context = new ConverterContext(_contentApiConfiguration.Default(), "name", null, true, page.Language);
                    var content = _contentLoader.Get<StandardPage>(page.ContentGuid);
                    var contentModel = _contentConverterService.Convert(content, context);
                    var json = _serializationResolver.Resolve(context.Options).Serialize(contentModel);
                    var jObject = JObject.Parse(json);
                    Assert.Equal(string.Format(CultureInfo.InvariantCulture, JsonMinifiedTemplate, page.ContentGuid, pageName), jObject.ToString());
                    return Task.CompletedTask;
                }
            });
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
