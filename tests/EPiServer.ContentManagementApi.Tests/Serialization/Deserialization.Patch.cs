using System;
using System.Collections.Generic;
using System.IO;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentManagementApi.Models.Internal;
using EPiServer.ContentManagementApi.Serialization.Internal;
using EPiServer.Core;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace EPiServer.ContentManagementApi.Serialization
{
    public class PatchDeserialization
    {
        private readonly JsonSerializer _jsonSerializer;

        public PatchDeserialization()
        {
            _jsonSerializer = new JsonSerializer();
            _jsonSerializer.Converters.Add(new ContentApiPatchModelJsonConverter());
        }

        [Theory]
        [MemberData(nameof(ExpectedContentApiModelDeserialization))]
        public void Deserialize_ShouldReturnExpectedFormat(string json, ContentApiPatchModel expectedModel)
        {
            using var stringReader = new StringReader(json);
            using var jsonReader = new JsonTextReader(stringReader);
            var contentApiModel = _jsonSerializer.Deserialize<ContentApiPatchModel>(jsonReader);

            contentApiModel.Should().BeEquivalentTo(expectedModel);

            Assert.Equal(contentApiModel.Properties["category"].ToString(),
                expectedModel.Properties["category"].ToString());

            Assert.Equal(contentApiModel.Properties["heading"].ToString(),
                expectedModel.Properties["heading"].ToString());

            Assert.Equal(contentApiModel.Properties["mainBody"].ToString(),
                expectedModel.Properties["mainBody"].ToString());

            Assert.Equal(contentApiModel.Properties["uri"].ToString(),
                expectedModel.Properties["uri"].ToString());
        }

        public static TheoryData ExpectedContentApiModelDeserialization => new TheoryData<string, ContentApiPatchModel>
        {
            {
                "{\"name\":\"Standard\"," +
                "\"language\":{\"displayName\":\"English\",\"name\":\"en\"}," +
                "\"routeSegment\":\"standard\"," +
                "\"startPublish\":\"2020-10-20 08:15:42+02:00\"," +
                "\"stopPublish\": null," +
                "\"status\":\"Published\"," +
                "\"category\":{\"value\":[{\"id\":1}]}," +
                "\"heading\":{\"value\":\"Standard Page\"}," +
                "\"mainBody\":{\"value\":\"<p>Main Body</p>\"}," +
                "\"uri\":{\"value\":\"/en/alloy-track/\"}," +
                "}",
                new ContentApiPatchModel
                {
                    Name = "Standard",
                    Language = new LanguageModel() { DisplayName = "English", Name = "en" },
                    RouteSegment = "standard",
                    StartPublish = DateTimeOffset.Parse("2020-10-20 08:15:42+02:00"),
                    StopPublish = null,
                    Status = VersionStatus.Published,
                    Properties = new Dictionary<string, object>() {
                        {
                            "category",
                            JObject.Parse("{\"value\":[{\"id\":1}]}")
                        },
                        {
                            "heading",
                            JObject.Parse("{\"value\":\"Standard Page\"}")
                        },
                        {
                            "mainBody",
                            JObject.Parse("{\"value\":\"<p>Main Body</p>\"}")
                        },
                        {
                            "uri",
                            JObject.Parse("{\"value\":\"/en/alloy-track/\"}")
                            
                        }
                    },
                    UpdatedMetadata = new HashSet<string>()
                    {
                        "RouteSegment", "StartPublish", "StopPublish", "Status"
                    }
                }
            }
        };
    }
}
