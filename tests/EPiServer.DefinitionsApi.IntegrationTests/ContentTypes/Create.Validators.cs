using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.ContentTypes;
using EPiServer.DefinitionsApi.IntegrationTests.ContentTypes.Models;
using EPiServer.DefinitionsApi.IntegrationTests.TestSetup;
using EPiServer.SpecializedProperties;
using EPiServer.Validation;
using FluentAssertions;
using Xunit;

namespace EPiServer.DefinitionsApi.IntegrationTests.ContentTypes
{
    public sealed partial class Create
    {
        [Fact]
        public async Task CreateAsync_WhenPropertyHasValidationSettings_ShouldAddPropertyValidationSettings()
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new[]
                {
                    new
                    {
                        name = "PropertyWithValidation",
                        dataType = nameof(PropertyString),
                        validation = new object[]
                        {
                            new
                            {
                                name = "length",
                                minimum = 5,
                                maximum = 20,
                                severity = "warning",
                                errorMessage = "Keep it real."
                            },
                            new
                            {
                                name = "regularExpression",
                                pattern = "\\d+",
                                severity = "info",
                                errorMessage = "We like numbers."
                            }
                        }
                    }
                }
            };

            using (_fixture.WithContentTypeIds(contentType.id))
            {
                await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

                // Assert
                var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + contentType.id);
                response.EnsureSuccessStatusCode();

                var created = await response.Content.ReadAsAsync<ExpectedContentType>();

                var expectedLengthValidation = new ExpectedPropertyValidationSettings
                {
                    Name = "Length",
                    ErrorMessage = "Keep it real.",
                    Severity = ValidationErrorSeverity.Warning,
                    Settings = new Dictionary<string, object>
                    {
                        { "Minimum", 5 },
                        { "Maximum", 20 },
                    },
                };

                var expectedRegularExpressionValidation = new ExpectedPropertyValidationSettings
                {
                    Name = "RegularExpression",
                    ErrorMessage = "We like numbers.",
                    Severity = ValidationErrorSeverity.Info,
                    Settings = new Dictionary<string, object>
                    {
                        { "Pattern", "\\d+" }
                    },
                };

                created.Properties.Should()
                    .ContainSingle(x => x.Name == "PropertyWithValidation")
                    .Which.Validation.Should()
                        .HaveCount(2)
                        .And.ContainEquivalentOf(expectedLengthValidation)
                        .And.ContainEquivalentOf(expectedRegularExpressionValidation);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenListPropertyHasItemValidationSettings_ShouldAddPropertyValidationSettings()
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new[]
                {
                    new
                    {
                        name = "PropertyWithValidation",
                        dataType = nameof(PropertyStringList),
                        validation = new[]
                        {
                            new
                            {
                                name = "length",
                                minimum = 2,
                                maximum = 5,
                                severity = "warning",
                                errorMessage = "Limit number of items."
                            },
                            new
                            {
                                name = "itemlength",
                                minimum = 5,
                                maximum = 20,
                                severity = "error",
                                errorMessage = "Limit length of the strings in the list."
                            },
                        }
                    }
                }
            };

            using (_fixture.WithContentTypeIds(contentType.id))
            {
                await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

                // Assert
                var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + contentType.id);
                response.EnsureSuccessStatusCode();

                var created = await response.Content.ReadAsAsync<ExpectedContentType>();

                var expectedLengthValidation = new ExpectedPropertyValidationSettings
                {
                    Name = "Length",
                    ErrorMessage = "Limit number of items.",
                    Severity = ValidationErrorSeverity.Warning,
                    Settings = new Dictionary<string, object>
                    {
                        { "Minimum", 2 },
                        { "Maximum", 5 },
                    },
                };

                var expectedItemLengthValidation = new ExpectedPropertyValidationSettings
                {
                    Name = "ItemLength",
                    ErrorMessage = "Limit length of the strings in the list.",
                    Severity = ValidationErrorSeverity.Error,
                    Settings = new Dictionary<string, object>
                    {
                        { "Minimum", 5 },
                        { "Maximum", 20 },
                    },
                };

                created.Properties.Should()
                    .ContainSingle(x => x.Name == "PropertyWithValidation")
                    .Which.Validation.Should()
                        .HaveCount(2)
                        .And.ContainEquivalentOf(expectedLengthValidation)
                        .And.ContainEquivalentOf(expectedItemLengthValidation);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenPropertyHasUnknownValidationSettingType_ShouldIgnoreSettings()
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new[]{
                    new {
                        name = "PropertyWithValidation",
                        dataType = nameof(PropertyString),
                        validation = new object[]
                        {
                            new { name = "unknown", unknownSetting = 59 },
                            new { name = "length", minimum = 2, severity = "error" },
                        }
                    }
                }
            };

            using (_fixture.WithContentTypeIds(contentType.id))
            {
                await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

                // Assert
                var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + contentType.id);
                response.EnsureSuccessStatusCode();

                var created = await response.Content.ReadAsAsync<ExpectedContentType>();

                var expectedLengthValidation = new ExpectedPropertyValidationSettings
                {
                    Name = "Length",
                    Severity = ValidationErrorSeverity.Error,
                    Settings = new Dictionary<string, object> { { "Minimum", 2 } }
                };

                created.Properties.Should()
                    .ContainSingle(x => x.Name == "PropertyWithValidation")
                    .Which.Validation.Should()
                        .ContainSingle()
                            .Which.Should()
                                .BeEquivalentTo(expectedLengthValidation);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenPropertyValidationIsMissingName_ShouldReturnBadRequest()
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new[]{
                    new {
                        name = "PropertyWithValidation",
                        dataType = nameof(PropertyString),
                        validation = new []
                        {
                            new { minimum = 2, severity = "error" },
                        }
                    }
                }
            };

            using (_fixture.WithContentTypeIds(contentType.id))
            {
                var response = await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

                AssertResponse.BadRequest(response);
            }
        }

        [Fact]
        public async Task CreateAsync_WhenPropertyHasUnknownValidationSettings_ShouldIgnoreSettings()
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new[]{
                    new {
                        name = "PropertyWithValidation",
                        dataType = nameof(PropertyString),
                        validation = new []
                        { 
                            new {
                                name = "length",
                                minimum = 2,
                                medium = 42, // This one doesn't exist
                                maximum = 100,
                                severity = "error"
                            },
                        }
                    }
                }
            };

            using (_fixture.WithContentTypeIds(contentType.id))
            {
                await _fixture.Client.PostAsync(ContentTypesController.RoutePrefix, new JsonContent(contentType));

                // Assert
                var response = await _fixture.Client.GetAsync(ContentTypesController.RoutePrefix + contentType.id);
                response.EnsureSuccessStatusCode();

                var created = await response.Content.ReadAsAsync<ExpectedContentType>();

                var expectedLengthValidation = new ExpectedPropertyValidationSettings
                {
                    Name = "Length",
                    Severity = ValidationErrorSeverity.Error,
                    Settings = new Dictionary<string, object> { { "Minimum", 2 }, { "Maximum", 100 } }
                };

                created.Properties.Should()
                    .ContainSingle(x => x.Name == "PropertyWithValidation")
                    .Which.Validation.Should()
                        .ContainSingle()
                            .Which.Should()
                                .BeEquivalentTo(expectedLengthValidation);
            }
        }
    }
}
