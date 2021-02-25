using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Licensing.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Internal
{
    public class ContentReferenceModelFilterTest
    {
        [Fact]
        public void Filter_WhenVersion2_ShouldNotFilterOutIdAndWorkId()
        {
            var id = 12;
            var workId = 14;
            var contentApiModel = new ContentApiModel { ContentLink = new ContentModelReference { Id = id, WorkId = workId } };
            var subject = new ContentReferenceModelFilter();
            subject.Filter(contentApiModel, CreateContext());
            Assert.Equal(id, contentApiModel.ContentLink.Id);
            Assert.Equal(workId, contentApiModel.ContentLink.WorkId);
        }

        [Fact]
        public void Filter_WhenVersion2_ShouldNotAddLanguageToContentLink()
        {
            var id = 12;
            var workId = 14;
            var contentApiModel = new ContentApiModel { ContentLink = new ContentModelReference { Id = id, WorkId = workId }, Language = new LanguageModel { Name = "en" } };
            var subject = new ContentReferenceModelFilter();
            subject.Filter(contentApiModel, CreateContext());
            Assert.Null(contentApiModel.ContentLink.Language);
        }

        [Fact]
        public void Filter_WhenVersion2_AndSetExcludeIdInfo_ShouldFilterOutIdAndWorkId()
        {
            var id = 12;
            var workId = 14;
            var contentApiModel = new ContentApiModel { ContentLink = new ContentModelReference { Id = id, WorkId = workId } };
            var subject = new ContentReferenceModelFilter();
            subject.Filter(contentApiModel, CreateContext(false));
            Assert.Null(contentApiModel.ContentLink.Id);
            Assert.Null(contentApiModel.ContentLink.WorkId);
        }

        [Fact]
        public void Filter_WhenVersion2_AndSetExcludeIdInfo_ShouldAddLanguageToContentLink()
        {
            var id = 12;
            var workId = 14;
            var contentApiModel = new ContentApiModel { ContentLink = new ContentModelReference { Id = id, WorkId = workId }, Language = new LanguageModel { Name = "en" } };
            var subject = new ContentReferenceModelFilter();
            subject.Filter(contentApiModel, CreateContext(false));
            Assert.Equal("en", contentApiModel.ContentLink.Language.Name);
        }

        [Fact]
        public void Filter_WhenModelHasExpandedContentReferenceProperty_ShouldFilterIdAndWorkIdOnExpanded()
        {
            var expandedContentApiModel = new ContentApiModel { ContentLink = new ContentModelReference { Id = 12, WorkId = 27 } };
            var rootContentApiModel = new ContentApiModel();
            rootContentApiModel.Properties.Add("Expanded", new ContentModelReference { Expanded = expandedContentApiModel });
            var subject = new ContentReferenceModelFilter();
            subject.Filter(rootContentApiModel, CreateContext());
            Assert.Null((rootContentApiModel.Properties["Expanded"] as ContentModelReference).Id);
            Assert.Null((rootContentApiModel.Properties["Expanded"] as ContentModelReference).WorkId);
        }

        [Fact]
        public void Filter_WhenModelHasExpandedContentReferenceList_ShouldFilterIdAndWorkIdOnExpanded()
        {
            var expandedContentApiModel = new ContentApiModel { ContentLink = new ContentModelReference { Id = 12, WorkId = 27 } };
            var rootContentApiModel = new ContentApiModel();
            rootContentApiModel.Properties.Add("Expanded", new[] { new ContentModelReference { Expanded = expandedContentApiModel } });
            var subject = new ContentReferenceModelFilter();
            subject.Filter(rootContentApiModel, CreateContext());
            Assert.Null((rootContentApiModel.Properties["Expanded"] as IEnumerable<ContentModelReference>).First().Id);
            Assert.Null((rootContentApiModel.Properties["Expanded"] as IEnumerable<ContentModelReference>).First().WorkId);
        }

        [Fact]
        public void Filter_WhenModelHasExpandedContentItems_ShouldFilterIdAndWorkIdOnExpanded()
        {
            var expandedContentApiModel = new ContentApiModel { ContentLink = new ContentModelReference { Id = 12, WorkId = 27 } };
            var rootContentApiModel = new ContentApiModel();
            rootContentApiModel.Properties.Add("Expanded", new[] { new TestableContentItem { ContentLink = new ContentModelReference { Expanded = expandedContentApiModel } } });
            var subject = new ContentReferenceModelFilter();
            subject.Filter(rootContentApiModel, CreateContext());
            Assert.Null((rootContentApiModel.Properties["Expanded"] as IEnumerable<IContentItem>).First().ContentLink.Id);
            Assert.Null((rootContentApiModel.Properties["Expanded"] as IEnumerable<IContentItem>).First().ContentLink.WorkId);
        }

        private ConverterContext CreateContext(bool includeContentId = true) => new ConverterContext(new ContentApiOptions().SetIncludeNumericContentIdentifier(includeContentId), "", "", true, CultureInfo.InvariantCulture);        
    }

    public class TestableContentItem : IContentItem
    {
        public ContentModelReference ContentLink { get; set; }
    }
}
