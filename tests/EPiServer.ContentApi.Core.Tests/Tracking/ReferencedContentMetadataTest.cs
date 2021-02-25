using EPiServer.ContentApi.Core.Tracking;
using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Tracking
{
    public class ReferencedContentMetadataTest
    {
        [Fact]
        public void Expires_WhenValueIsNotSet_ShouldNotHaveValue()
        {
            var refContentMetadata = new ReferencedContentMetadata();
            Assert.Null(refContentMetadata.ExpirationTime);
        }

        [Fact]
        public void Expires_WhenValueIsSet_ShouldHaveValue()
        {
            var refContentMetadata = new ReferencedContentMetadata() { ExpirationTime = DateTime.UtcNow };
            Assert.NotNull(refContentMetadata.ExpirationTime);
        }

        [Fact]
        public void Saved_WhenValueIsNotSet_ShouldNotHaveValue()
        {
            var refContentMetadata = new ReferencedContentMetadata();
            Assert.Null(refContentMetadata.SavedTime);
        }

        [Fact]
        public void Saved_WhenValueIsSet_ShouldHaveValue()
        {
            var refContentMetadata = new ReferencedContentMetadata() { SavedTime = DateTime.UtcNow };
            Assert.NotNull(refContentMetadata.SavedTime);
        }

        [Fact]
        public void PersonalizedProperties_WhenNoItemsHaveBeenAdded_ShouldBeEmpty()
        {
            var subject = new ReferencedContentMetadata();
            Assert.Empty(subject.PersonalizedProperties);
        }

        [Fact]
        public void PersonalizedProperties_WhenOnePropertyIsAdded_ShouldHaveOneProperty()
        {
            var subject = new ReferencedContentMetadata();
            subject.PersonalizedProperties.Add("prop1");
            Assert.Single(subject.PersonalizedProperties);
            Assert.Equal("prop1", subject.PersonalizedProperties.Single());
        }

        [Fact]
        public void PersonalizedProperties_WhenTwoDifferentPropertiesIsAdded_ShouldHaveTwoProperties()
        {
            var subject = new ReferencedContentMetadata();
            subject.PersonalizedProperties.Add("prop1");
            subject.PersonalizedProperties.Add("prop2");
            Assert.Equal(2, subject.PersonalizedProperties.Count);
            Assert.Contains("prop1", subject.PersonalizedProperties);
            Assert.Contains("prop2", subject.PersonalizedProperties);
        }

        [Fact]
        public void PersonalizedProperties_WhenSamePropertyIsAddedTwice_ShouldNotAddSecondItem()
        {
            var subject = new ReferencedContentMetadata();
            subject.PersonalizedProperties.Add("prop1");
            subject.PersonalizedProperties.Add("prop1");
            Assert.Equal("prop1", subject.PersonalizedProperties.Single());
        }

        private LanguageContentReference CreateLanguageContentReference(int contentID = 1, int versionID = 0)
                => new LanguageContentReference(new ContentReference(contentID, versionID), CultureInfo.GetCultureInfo("en"));
    }
}
