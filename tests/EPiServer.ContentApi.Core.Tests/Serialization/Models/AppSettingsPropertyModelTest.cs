using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.SpecializedProperties;
using System;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class AppSettingsPropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AppSettingsPropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            var appSettingsProperty = new PropertyAppSettings();

            var appSettingsPropertyModel = new AppSettingsPropertyModel(appSettingsProperty);

            Assert.NotNull(appSettingsPropertyModel);
        }
    }
}
