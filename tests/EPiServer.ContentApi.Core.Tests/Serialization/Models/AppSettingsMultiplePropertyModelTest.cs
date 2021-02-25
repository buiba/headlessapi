using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.SpecializedProperties;
using System;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class AppSettingsMultiplePropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AppSettingsMultiplePropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            var appSettingsMultipleProperty = new PropertyAppSettingsMultiple();
            
            var appSettingsMultiplePropertyModel = new AppSettingsMultiplePropertyModel(appSettingsMultipleProperty);
            
            Assert.NotNull(appSettingsMultiplePropertyModel);
        }
    }
}
