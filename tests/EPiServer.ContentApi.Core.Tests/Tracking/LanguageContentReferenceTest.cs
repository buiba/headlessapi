using EPiServer.ContentApi.Core.Tracking;
using EPiServer.Core;
using System.Globalization;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Tracking
{
    public class LanguageContentReferenceTest
    {
        [Fact]
        public void Equals_WhenTwoReferencesDifferInBothContentAndLanguage_ShouldNotBeEqual()
        {
            var subject1 = Subject("en", 1);
            var subject2 = Subject("sv", 2);
            Assert.NotEqual(subject1, subject2);
        }

        [Fact]
        public void Equals_WhenTwoReferencesDifferInContent_ShouldNotBeEqual()
        {
            var subject1 = Subject("en", 1);
            var subject2 = Subject("en", 2);
            Assert.NotEqual(subject1, subject2);
        }

        [Fact]
        public void Equals_WhenTwoReferencesDifferInLanguage_ShouldNotBeEqual()
        {
            var subject1 = Subject("en", 1);
            var subject2 = Subject("sv", 1);
            Assert.NotEqual(subject1, subject2);
        }

        [Fact]
        public void Equals_WhenTwoReferencesHasTheSameContentAndLanguage_ShouldBeEqual()
        {
            var subject1 = Subject("en", 1);
            var subject2 = Subject("en", 1);
            Assert.Equal(subject1, subject2);
        }

        [Fact]
        public void Equals_WhenTwoReferencesDifferInVersion_ShouldNotBeEqual()
        {
            var subject1 = Subject("en", 1, 3);
            var subject2 = Subject("en", 1, 4);
            Assert.NotEqual(subject1, subject2);
        }

        [Fact]
        public void Equals_WhenTwoReferencesHasTheSameVersionAndLanguage_ShouldBeEqual()
        {
            var subject1 = Subject("en", 1, 3);
            var subject2 = Subject("en", 1, 3);
            Assert.Equal(subject1, subject2);
        }

        [Fact]
        public void Equals_WhenTwoReferencesDiffersBetweenContentAndVersion_ShouldNotBeEqual()
        {
            var subject1 = Subject("en", 1);
            var subject2 = Subject("en", 1, 3);
            Assert.NotEqual(subject1, subject2);
        }

        private LanguageContentReference Subject(string language, int contentID, int? versionID = null)
        {
            var cultureInfo = CultureInfo.GetCultureInfo(language);
            var contentLink = versionID.HasValue ? new ContentReference(contentID, versionID.Value) : new ContentReference(contentID);
            return new LanguageContentReference(contentLink, cultureInfo);
        }
    }
}
