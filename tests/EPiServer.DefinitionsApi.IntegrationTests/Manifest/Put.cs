using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.DefinitionsApi.Manifest;
using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;
using Xunit;

namespace EPiServer.DefinitionsApi.IntegrationTests.Manifest
{
    [Collection(IntegrationTestCollection.Name)]
    public class Put
    {
        private readonly ServiceFixture _fixture;

        public Put(ServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Put_WhenNoSections_ShouldReturnValidationError()
        {
            var model = new { };

            var response = await _fixture.Client.PutAsync(ManifestController.RoutePrefix, new JsonContent(model));

            AssertResponse.ValidationError(response);
        }

        [Fact]
        public async Task Put_ShouldImportContentTypes()
        {
            var model = new
            {
                contentTypes = new
                {
                    items = new[]
                    {
                        new
                        {
                            baseType = "Page",
                            name = "PageType1"
                        },
                        new
                        {
                            baseType = "Page",
                            name = "PageType2"
                        }
                    }
                }
            };

            var response = await _fixture.Client.PutAsync(ManifestController.RoutePrefix, new JsonContent(model));
            var result = await response.Content.ReadAs<IEnumerable<ImportLogMessage>>().ConfigureAwait(false);

            Assert.Single(result);

            var contentTypes = _fixture.ContentTypeRepository.List();

            Assert.NotNull(contentTypes.SingleOrDefault(x => x.Name == "PageType1"));
            Assert.NotNull(contentTypes.SingleOrDefault(x => x.Name == "PageType2"));
        }

        [Fact]
        public async Task Put_WhenPageTypeHasBlockTypeProperty_ShouldImportContentTypes()
        {
            var model = new
            {
                contentTypes = new
                {
                    items = new[]
                    {
                        new
                        {
                            baseType = "Block",
                            name = "BlockTypeB",
                            properties = new object[0]
                        },
                        new
                        {
                            baseType = "Page",
                            name = "PageTypeA",
                            properties = new object[]
                            {
                                new { name = "BlockProperty", dataType = nameof(PropertyBlock), itemType = "BlockTypeB"  }
                            }
                        }
                    }
                }
            };

            var response = await _fixture.Client.PutAsync(ManifestController.RoutePrefix, new JsonContent(model));

            AssertResponse.OK(response);

            var result = await response.Content.ReadAs<IEnumerable<ImportLogMessage>>().ConfigureAwait(false);

            Assert.Single(result);

            var contentTypes = _fixture.ContentTypeRepository.List();

            Assert.Contains(contentTypes, x => x.Name == "PageTypeA");
            Assert.Contains(contentTypes, x => x.Name == "BlockTypeB");
        }

        [Fact(Skip = "Requires CMS Core fix for bug CMS-17262.")]
        public async Task Put_WhenBlockTypeHasBlockTypeProperty_ShouldImportContentTypes()
        {
            var model = new
            {
                contentTypes = new
                {
                    items = new[]
                    {
                        new
                        {
                            baseType = "Block",
                            name = "BlockTypeA",
                            properties = new object[]
                            {
                                new { name = "BlockProperty", dataType = nameof(PropertyBlock), itemType = "BlockTypeB"  }
                            }
                        },
                        new
                        {
                            baseType = "Block",
                            name = "BlockTypeB",
                            properties = new object[0]
                        }
                    }
                }
            };

            var response = await _fixture.Client.PutAsync(ManifestController.RoutePrefix, new JsonContent(model));

            AssertResponse.OK(response);

            var result = await response.Content.ReadAs<IEnumerable<ImportLogMessage>>().ConfigureAwait(false);

            Assert.Single(result);

            var contentTypes = _fixture.ContentTypeRepository.List();

            Assert.Contains(contentTypes, x => x.Name == "BlockTypeA");
            Assert.Contains(contentTypes, x => x.Name == "BlockTypeB");
        }

        [Fact]
        public async Task Put_ShouldImportPropertyGroups()
        {
            var model = new
            {
                propertyGroups = new
                {
                    items = new[]
                    {
                        new
                        {
                            name = "PropertyGroup1"
                        },
                        new
                        {
                            name = "PropertyGroup2",
                        }
                    }
                }
            };

            var response = await _fixture.Client.PutAsync(ManifestController.RoutePrefix, new JsonContent(model));
            var result = await response.Content.ReadAs<IEnumerable<ImportLogMessage>>().ConfigureAwait(false);

            Assert.Single(result);

            var tabDefinitions = _fixture.TabDefinitionRepository.List();

            Assert.NotNull(tabDefinitions.SingleOrDefault(x => x.Name == "PropertyGroup1"));
            Assert.NotNull(tabDefinitions.SingleOrDefault(x => x.Name == "PropertyGroup2"));
        }

        [Fact]
        public async Task Put_WhenInvalidSection_ShouldReturnValidationError()
        {
            var model = new
            {
                contentTypes = new
                {
                    items = new[]
                    {
                        new
                        {
                            baseType = "Page",
                            name = "PageType3"
                        },
                        new
                        {
                            baseType = "InvalidType", // Invalid type
                            name = "PageType4"
                        }
                    }
                },
                propertyGroups = new
                {
                    items = new[]
                    {
                        new
                        {
                            name = "PropertyGroup3"
                        },
                        new
                        {
                            name = "PropertyGroup4",
                        }
                    }
                }
            };

            var response = await _fixture.Client.PutAsync(ManifestController.RoutePrefix, new JsonContent(model));

            AssertResponse.ValidationError(response);
        }

        [Fact]
        public async Task Put_WhenSectionImporterFailsAndContinueOnError_ShouldContinueWithNextSectionImporter()
        {
            var model = new
            {
                failingSection = new { },
                contentTypes = new
                {
                    items = new[]
                    {
                        new
                        {
                            baseType = "Page",
                            name = "PageType5"
                        },
                        new
                        {
                            baseType = "Page",
                            name = "PageType6"
                        }
                    }
                },
                propertyGroups = new
                {
                    items = new[]
                    {
                        new
                        {
                            name = "PropertyGroup5"
                        },
                        new
                        {
                            name = "PropertyGroup6",
                        }
                    }
                }
            };

            var response = await _fixture.Client.PutAsync(ManifestController.RoutePrefix, new JsonContent(model));
            var result = await response.Content.ReadAs<IEnumerable<ImportLogMessage>>().ConfigureAwait(false);

            Assert.Collection(
                result,
                x => Assert.Equal(ImportLogMessageSeverity.Error, x.Severity),
                x => Assert.Equal(ImportLogMessageSeverity.Success, x.Severity),
                x => Assert.Equal(ImportLogMessageSeverity.Success, x.Severity));

            var contentTypes = _fixture.ContentTypeRepository.List();

            Assert.NotNull(contentTypes.SingleOrDefault(x => x.Name == "PageType5"));
            Assert.NotNull(contentTypes.SingleOrDefault(x => x.Name == "PageType6"));

            var tabDefinitions = _fixture.TabDefinitionRepository.List();

            Assert.NotNull(tabDefinitions.SingleOrDefault(x => x.Name == "PropertyGroup5"));
            Assert.NotNull(tabDefinitions.SingleOrDefault(x => x.Name == "PropertyGroup6"));
        }

        [Fact]
        public async Task Put_WhenSectionImporterFailsAndNotContinueOnError_ShouldReturnBadRequest()
        {
            var model = new
            {
                failingSection = new { },
                contentTypes = new
                {
                    items = new[]
                    {
                        new
                        {
                            baseType = "Page",
                            name = "PageType7"
                        },
                        new
                        {
                            baseType = "Page",
                            name = "PageType8"
                        }
                    }
                },
                propertyGroups = new
                {
                    items = new[]
                    {
                        new
                        {
                            name = "PropertyGroup7"
                        },
                        new
                        {
                            name = "PropertyGroup8",
                        }
                    }
                }
            };

            var response = await _fixture.Client.PutAsync(ManifestController.RoutePrefix + "?continueOnError=false", new JsonContent(model));

            AssertResponse.BadRequest(response);
        }

        [ServiceConfiguration(typeof(IManifestSectionImporter), Lifecycle = ServiceInstanceScope.Singleton)]
        private class FailingSectionImporter : IManifestSectionImporter
        {
            public Type SectionType => typeof(FailingSection);

            public string SectionName => nameof(FailingSection);

            public int Order => 1;

            public void Import(IManifestSection section, ImportContext importContext)
            {
                throw new Exception();
            }
        }

        private class FailingSection : IManifestSection
        { }
    }
}
