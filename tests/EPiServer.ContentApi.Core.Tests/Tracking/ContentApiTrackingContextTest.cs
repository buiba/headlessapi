using EPiServer.ContentApi.Core.Tracking;
using EPiServer.Core;
using System.Globalization;
using System.Linq;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Tracking
{
    public class ContentApiTrackingContextTest
    {
        [Fact]
        public void ReferencedContent_WhenNoItemsHaveBeenAdded_ShouldBeEmpty()
        {
            var subject = new ContentApiTrackingContext();
            Assert.Empty(subject.ReferencedContent);
        }

        [Fact]
        public void ReferencedContent_WhenOneItemIsAdded_ShouldHaveOneValue()
        {
            var subject = new ContentApiTrackingContext();
            var reference = CreateLanguageContentReference();
            subject.ReferencedContent.Add(reference, new ReferencedContentMetadata());
            Assert.Single(subject.ReferencedContent);
            Assert.Equal(reference, subject.ReferencedContent.Single().Key);
        }

        [Fact]
        public void ReferencedContent_WhenTwoDifferentItemsAreAdded_ShouldHaveValues()
        {
            var subject = new ContentApiTrackingContext();
            var reference1 = CreateLanguageContentReference(1);
            var reference2 = CreateLanguageContentReference(2);
            subject.ReferencedContent.Add(reference1, new ReferencedContentMetadata());
            subject.ReferencedContent.Add(reference2, new ReferencedContentMetadata());
            Assert.Equal(2, subject.ReferencedContent.Count);
            Assert.True(subject.ReferencedContent.ContainsKey(reference1));
            Assert.True(subject.ReferencedContent.ContainsKey(reference2));
            Assert.NotEqual(subject.ReferencedContent.First(), subject.ReferencedContent.Last());
        }

        [Fact]
        public void ReferencedContent_WhenMetadataIsNull_ShouldNotHaveValue()
        {
            var subject = new ContentApiTrackingContext();
            var reference = CreateLanguageContentReference(1);
            subject.ReferencedContent.Add(reference, null);
            Assert.Single(subject.ReferencedContent);
            Assert.Null(subject.ReferencedContent.Single().Value);
        }

        [Fact]
        public void ReferencedContent_WhenMetadataIsNotNull_ShouldHaveValue()
        {
            var subject = new ContentApiTrackingContext();
            var reference = CreateLanguageContentReference(1);
            var refMetadata = new ReferencedContentMetadata();
            subject.ReferencedContent.Add(reference, refMetadata);
            Assert.Single(subject.ReferencedContent);
            Assert.Equal(refMetadata, subject.ReferencedContent.Single().Value);
        }

        [Fact]
        public void SecuredContent_WhenNoItemsHaveBeenAdded_ShouldBeEmpty()
        {
            var subject = new ContentApiTrackingContext();
            Assert.Empty(subject.SecuredContent);
        }

        [Fact]
        public void SecuredContent_WhenOneItemIsAdded_ShouldHaveOneValue()
        {
            var subject = new ContentApiTrackingContext();
            var reference = new ContentReference(1);
            subject.SecuredContent.Add(reference);
            Assert.Single(subject.SecuredContent);
            Assert.Equal(reference, subject.SecuredContent.Single());
        }

        [Fact]
        public void SecuredContent_WhenTwoDifferentItemsAreAdded_ShouldHaveValues()
        {
            var subject = new ContentApiTrackingContext();
            var reference1 = new ContentReference(1);
            var reference2 = new ContentReference(2);
            subject.SecuredContent.Add(reference1);
            subject.SecuredContent.Add(reference2);
            Assert.Equal(2, subject.SecuredContent.Count);
            Assert.Contains(reference1, subject.SecuredContent);
            Assert.Contains(reference2, subject.SecuredContent);
            Assert.NotEqual(subject.SecuredContent.First(), subject.SecuredContent.Last());
        }

        [Fact]
        public void SecuredContent_WhenItemIsAddedTwice_ShouldNotAddSecondItem()
        {
            var subject = new ContentApiTrackingContext();
            var reference1a = new ContentReference(1);
            var reference1b = new ContentReference(1);
            subject.SecuredContent.Add(reference1a);
            subject.SecuredContent.Add(reference1b);
            Assert.Single(subject.SecuredContent);
            Assert.Equal(reference1a, subject.SecuredContent.Single());
        }

        private LanguageContentReference CreateLanguageContentReference(int contentID = 1, int versionID = 0)
            => new LanguageContentReference(new ContentReference(contentID, versionID), CultureInfo.GetCultureInfo("en"));
    }
}
