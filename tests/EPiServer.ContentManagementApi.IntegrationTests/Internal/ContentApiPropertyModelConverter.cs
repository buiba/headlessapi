using System;
using System.Collections.Generic;
using System.Net;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Properties;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.ContentManagementApi.Models.Internal;
using EPiServer.ContentManagementApi.Serialization.Internal;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace EPiServer.ContentManagementApi.IntegrationTests.Internal
{
    [Collection(IntegrationTestCollection.Name)]
    public class ContentApiPropertyModelConverterTest
    {
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly ContentApiPropertyModelConverter _contentApiPropertyModelConverter;

        public ContentApiPropertyModelConverterTest()
        {
            _contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
            var typeModelResolver = ServiceLocator.Current.GetInstance<TypeModelResolver>();
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

            _contentApiPropertyModelConverter = new ContentApiPropertyModelConverter(_contentTypeRepository, typeModelResolver, contentRepository);
        }

        [Theory]
        [MemberData(nameof(ExpectedContentApiModelDeserializations))]
        public void TransformProperties_ShouldTransformToCorrespondingPropertyModels(IDictionary<string, object> inputProperties, IDictionary<string, object> expectedProperties)
        {
            var properties = _contentApiPropertyModelConverter.ConvertRawPropertiesToPropertyModels(
                _contentTypeRepository.Load(nameof(StandardPageWithBlock)),
                inputProperties,
                new JsonSerializer());

            properties.Should().BeEquivalentTo(expectedProperties);
        }

        [Fact]
        public void TransformProperties_ShouldIgnorePropertyDataTypeInBlock()
        {
            var inputProperties = new Dictionary<string, object>()
            {
                {"localBlock", JObject.Parse("{\"name\":\"SomeBlock\",\"heading\":\"Some Heading\",\"propertyDataType\":\"PropertyBlock\"}")}
            };

            var properties = _contentApiPropertyModelConverter.ConvertRawPropertiesToPropertyModels(
                _contentTypeRepository.Load(nameof(StandardPageWithBlock)),
                inputProperties,
                new JsonSerializer());

            var blockPropertyModel = properties["LocalBlock"] as BlockPropertyModel;
            Assert.Equal(1, blockPropertyModel.Properties.Count);
            Assert.True(blockPropertyModel.Properties.ContainsKey("Heading"));
        }

        [Fact]
        public void TransformProperties_WhenPropertyDefinitionDoesNotExist_ShouldThrow()
        {
            var inputProperties = new Dictionary<string, object>()
            {
                {"testProperty", JObject.Parse("{\"value\":\"Test Property\"}")}
            };

            var exception = Assert.Throws<ErrorException>(() => _contentApiPropertyModelConverter.ConvertRawPropertiesToPropertyModels(
                _contentTypeRepository.Load(nameof(StandardPageWithCustomPropertyLongString)),
                inputProperties,
                new JsonSerializer()));

            exception.StatusCode.Equals(HttpStatusCode.BadRequest);
            exception.Message.Should().BeEquivalentTo($"Property definition for 'testProperty' does not exist.");
        }

        [Fact]
        public void TransformProperties_WhenPropertyValueIsNotValid_ShouldThrow()
        {
            var inputProperties = new Dictionary<string, object>()
            {
                {"mainContentArea", JObject.Parse("{\"value\":\"Abc\"}")}
            };

            var exception = Assert.Throws<ErrorException>(() => _contentApiPropertyModelConverter.ConvertRawPropertiesToPropertyModels(
                _contentTypeRepository.Load(nameof(StandardPage)),
                inputProperties,
                new JsonSerializer()));

            exception.StatusCode.Equals(HttpStatusCode.BadRequest);
            exception.Message.Should().Contain("Error converting value");
        }

        [Fact]
        public void TransformProperties_WhenHasNoCorrespondingPropertyModel_ShouldThrow()
        {
            var inputProperties = new Dictionary<string, object>()
            {
                {"customProperty", JObject.Parse("{\"value\":\"Custom Property\"}")}
            };

            var exception = Assert.Throws<ErrorException>(() => _contentApiPropertyModelConverter.ConvertRawPropertiesToPropertyModels(
                _contentTypeRepository.Load(nameof(StandardPageWithCustomPropertyLongString)),
                inputProperties,
                new JsonSerializer()));

            exception.StatusCode.Equals(HttpStatusCode.BadRequest);
            exception.Message.Should().BeEquivalentTo($"Cannot resolve IPropertyModel for '{typeof(CustomPropertyLongString).FullName}'.");
        }


        [Fact]
        public void TransformProperties_WhenIdIsNotFound_ShouldThrow()
        {
            var exception = Assert.Throws<ErrorException>(() => _contentApiPropertyModelConverter.ConvertRawPropertiesToPropertyModels(
                "19999",
                new Dictionary<string, object>(),
                new JsonSerializer()));

            exception.StatusCode.Equals(HttpStatusCode.NotFound);
            exception.Message.Should().BeEquivalentTo($"The content with id (19999) does not exist.");
        }

        [Fact]
        public void TransformProperties_WhenInputContentTypeDoesNotExist_ShouldThrow()
        {
            var contentApiCreateModel = new ContentApiCreateModel
            {
                Name = "Test Content",
                ContentType = new List<string> { "UnknownContentType" },
                ParentLink = new ContentReferenceInputModel { Id = 1 },
            };

            var exception = Assert.Throws<ErrorException>(() => _contentApiPropertyModelConverter.ConvertRawPropertiesToPropertyModels(
                contentApiCreateModel.ContentType,
                contentApiCreateModel.Properties,
                new JsonSerializer()));

            exception.StatusCode.Equals(HttpStatusCode.BadRequest);
            exception.Message.Equals("Content type 'UnknownContentType' does not exist.");
        }

        [Theory]
        [MemberData(nameof(NullOrEmptyContentType))]
        public void TransformProperties_WhenInputContentTypeIsNullOrEmpty_ShouldThrow(List<string> contentTypes)
        {
            var contentApiCreateModel = new ContentApiCreateModel
            {
                Name = "Test Content",
                ContentType = contentTypes,
                ParentLink = new ContentReferenceInputModel { Id = 1 },
            };

            var exception = Assert.Throws<ErrorException>(() => _contentApiPropertyModelConverter.ConvertRawPropertiesToPropertyModels(
                contentApiCreateModel.ContentType,
                contentApiCreateModel.Properties,
                new JsonSerializer()));

            exception.StatusCode.Equals(HttpStatusCode.BadRequest);
            exception.Message.Equals("Content type is required.");
        }

        public static TheoryData ExpectedContentApiModelDeserializations =>
            new TheoryData<IDictionary<string, object>, IDictionary<string, object>>
            {
                {
                    new Dictionary<string, object>()
                    {
                        {"category", JObject.Parse("{\"value\":[{\"id\":1}]}")},
                        {"heading", JObject.Parse("{\"value\":\"Standard Page\"}")},
                        {"mainBody", JObject.Parse("{\"value\":\"<p>Main Body</p>\"}")},
                        {"uri", JObject.Parse("{\"value\":\"/en/alloy-track/\"}")},
                        {"links", null},
                        {"targetReference", JObject.Parse("{\"abc\" : \"123\"}")},
                        {"localBlock", JObject.Parse("{\"name\":\"SomeBlock\"," +
                            "\"propertyDataType\":\"PropertyBlock\"," +
                            "\"textLink\":{\"value\":\"/some/url\"}," +
                            "\"heading\":{\"value\":\"Some Block Title\"}," +
                            "\"notFlattenableProperty\":{" +
                                "\"name\":\"SomeNestedBlock\"," +
                                "\"propertyDataType\":\"PropertyBlock\"," +
                                "\"title\":{\"value\":\"Some Nested Block Title\"}}}")}
                    },
                    new Dictionary<string, object>()
                    {
                        {"Category", new CategoryPropertyModel() {Value = new[] {new CategoryModel() {Id = 1}}}},
                        {"Heading", new LongStringPropertyModel() {Value = "Standard Page"}},
                        {"MainBody", new XHtmlPropertyModel() {Value = "<p>Main Body</p>"}},
                        {"Uri", new UrlPropertyModel() {Value = "/en/alloy-track/"}},
                        {"Links", null},
                        {"TargetReference", new ContentReferencePropertyModel() {Value = null}},
                        {"LocalBlock", new BlockPropertyModel()
                            {
                                Name = "SomeBlock",
                                Properties = new Dictionary<string, object>()
                                {
                                    {"TextLink", new UrlPropertyModel() {Value = "/some/url"}},
                                    {"Heading", new LongStringPropertyModel() {Value = "Some Block Title"}},
                                    {
                                        "NotFlattenableProperty", new BlockPropertyModel()
                                        {
                                            Name = "SomeNestedBlock",
                                            Properties = new Dictionary<string, object>()
                                            {
                                                {
                                                    "Title",
                                                    new LongStringPropertyModel() {Value = "Some Nested Block Title"}
                                                }
                                            }
                                        }
                                    },
                                }
                            }
                        },

                    }
                }
            };

        public static TheoryData NullOrEmptyContentType =>
            new TheoryData<List<string>>
            {
                null,
                new List<string>()
            };
    }
}
