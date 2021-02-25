using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.SpecializedProperties;
using System;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Models
{
    public class LanguagePropertyModelTest
    {
        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new LanguagePropertyModel(null));
        }

        [Fact]
        public void Constructor()
        {
            var propertyLanguage = new PropertyLanguage();

            var languagePropertyModel = new LanguagePropertyModel(propertyLanguage);

            Assert.NotNull(languagePropertyModel);
        }
    }
}
