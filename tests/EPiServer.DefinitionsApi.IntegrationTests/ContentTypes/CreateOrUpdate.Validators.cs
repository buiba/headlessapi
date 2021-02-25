using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.IntegrationTests.ContentTypes.Models;
using EPiServer.DefinitionsApi.IntegrationTests.TestSetup;
using EPiServer.Validation;
using FluentAssertions;
using Xunit;

namespace EPiServer.DefinitionsApi.IntegrationTests.ContentTypes
{
    public sealed partial class CreateOrUpdate //.Validators
    {
        [Fact]
        public async Task CreateOrUpdateAsync_WhenValidationSettingsAreAddedToProperty_ShouldAddValidationSettings()
        {
            var contentType = new
            {
                id = Guid.NewGuid(),
                name = $"ContentType_{Guid.NewGuid():N}",
                baseType = ContentTypeBase.Page.ToString(),
                properties = new []{
                    new {
                        name = "PropertyWithValidation",
                        dataType = nameof(PropertyString),
                        validation = new List<object>()
                    }
                }
            };

            using (_fixture.WithContentTypeIds(contentType.id))
            {
                await CreateContentType(contentType);

                // Add validation
                contentType.properties[0].validation.Add(new
                    {
                        name = "length",
                        minimum = 5,
                        severity = "info",
                        errorMessage = "Not too small."
                    });

                // Act
                await CallCreateOrUpdateAsync(contentType.id, contentType);

                // Assert
                var response = await GetContentType(contentType.id);

                var updated = await response.Content.ReadAsAsync<ExpectedContentType>();

                var expectedValidation = new ExpectedPropertyValidationSettings
                {
                    Name = "Length",
                    ErrorMessage = "Not too small.",
                    Severity = ValidationErrorSeverity.Info,
                    Settings = new Dictionary<string, object>
                    {
                        { "Minimum", 5 },
                    },
                };

                updated.Properties.Should()
                    .ContainSingle(x => x.Name == "PropertyWithValidation")
                    .Which.Validation.Should()
                        .ContainSingle()
                            .Which.Should()
                                .BeEquivalentTo(expectedValidation);
            }
        }

        [Fact]
        public async Task CreateOrUpdateAsync_WhenExistingValidationSettingsAreUpdated_ShouldUpdateValidationSettings()
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
                            new ExpectedPropertyValidationSettings
                            {
                                Name = "length",
                                Severity = ValidationErrorSeverity.Error,
                                Settings = new Dictionary<string, object> {
                                    { "minimum", 5 },
                                    { "maximum", 20 },
                                }
                            },
                        }
                    }
                }
            };

            using (_fixture.WithContentTypeIds(contentType.id))
            {
                await CreateContentType(contentType);

                // Add validation
                contentType.properties[0].validation[0].Settings["maximum"] = 100;

                // Act
                await CallCreateOrUpdateAsync(contentType.id, contentType);

                // Assert
                var response = await GetContentType(contentType.id);

                var updated = await response.Content.ReadAsAsync<ExpectedContentType>();

                updated.Properties.Should()
                    .ContainSingle(x => x.Name == "PropertyWithValidation")
                    .Which.Validation.Should()
                        .ContainSingle()
                            .Which.Settings.Should()
                                .Contain("Maximum", 100);
            }
        }

        [Fact]
        public async Task CreateOrUpdateAsync_WhenExistingValidationSettingsIsMissingIndividualSetting_ShouldUpdateValidationSettings()
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
                            new ExpectedPropertyValidationSettings
                            {
                                Name = "Length",
                                Severity = ValidationErrorSeverity.Error,
                                Settings = new Dictionary<string, object> {
                                    { "minimum", 5 },
                                    { "maximum", 20 },
                                }
                            },
                        }
                    }
                }
            };

            using (_fixture.WithContentTypeIds(contentType.id))
            {
                await CreateContentType(contentType);

                // Add validation
                contentType.properties[0].validation[0].Settings.Remove("maximum");

                // Act
                await CallCreateOrUpdateAsync(contentType.id, contentType);

                // Assert
                var response = await GetContentType(contentType.id);

                var updated = await response.Content.ReadAsAsync<ExpectedContentType>();

                updated.Properties.Should()
                    .ContainSingle(x => x.Name == "PropertyWithValidation")
                    .Which.Validation.Should()
                        .ContainSingle()
                            .Which.Settings.Should()
                                .NotContainKey("Maximum");
            }
        }

        [Fact]
        public async Task CreateOrUpdateAsync_WhenAllValidationSettingsAreRemovedFromToProperty_ShouldRemoveValidationSettings()
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
                        validation = new List<object>
                        {
                            new { name = "length", minimum = 5 }
                        }
                    }
                }
            };

            using (_fixture.WithContentTypeIds(contentType.id))
            {
                await CreateContentType(contentType);

                // Clear validation
                contentType.properties[0].validation.Clear();

                // Act
                await CallCreateOrUpdateAsync(contentType.id, contentType);

                // Assert
                var response = await GetContentType(contentType.id);

                var created = await response.Content.ReadAsAsync<ExpectedContentType>();

                created.Properties.Should()
                    .ContainSingle(x => x.Name == "PropertyWithValidation")
                    .Which.Validation.Should().BeNull();
            }
        }
    }
}
