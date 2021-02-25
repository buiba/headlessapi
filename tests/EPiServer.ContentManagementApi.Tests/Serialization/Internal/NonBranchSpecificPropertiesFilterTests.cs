using System;
using System.Collections.Generic;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.DataAbstraction;
using EPiServer.Security;
using EPiServer.Web;
using Moq;
using Xunit;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    public class NonBranchSpecificPropertiesFilterTests
    {
        private const string ContentAreaProp = "ContentAreaProp";
        private const string DateTimeProp = "DateTimeProp";
        private const string BooleanProp = "BooleanProp";
        private const string FloatProp = "FloatProp";

        private readonly Mock<IContentTypeRepository> _contentTypeRepository;
        private readonly NonBranchSpecificPropertiesFilter _subject;
        private readonly ContentApiModel _contentApiModel;
        private readonly ContentType _contentType;

        public NonBranchSpecificPropertiesFilterTests()
        {
            _contentTypeRepository = new Mock<IContentTypeRepository>();
            _subject = new NonBranchSpecificPropertiesFilter(_contentTypeRepository.Object);

            _contentApiModel = new ContentApiModel
            {
                ContentLink = new ContentModelReference { Id = 1, Language = new LanguageModel { Name = "en" } },
                ContentType = new List<string> { "ContactPage" }
            };
            _contentApiModel.MasterLanguage = new LanguageModel { Name = "en" };
            _contentApiModel.Language = new LanguageModel { Name = "sv" };
            _contentApiModel.Properties = new Dictionary<string, object>();

            var keyWithValue = "akey";
            var keyWithNullValue = "anotherkey";

            var blockModel = new ContentApiModel();
            blockModel.Properties[keyWithValue] = "something";
            blockModel.Properties[keyWithNullValue] = null;
            var expandedContentReference = new ContentReferencePropertyModel(new EPiServer.Core.PropertyContentReference(), new TestConverterContext(false), Mock.Of<ContentLoaderService>(), Mock.Of<ContentConvertingService>(), Mock.Of<IContentAccessEvaluator>(), Mock.Of<ISecurityPrincipal>(), Mock.Of<UrlResolverService>())
            {
                ExpandedValue = blockModel
            };

            _contentApiModel.Properties.Add(ContentAreaProp, new List<ContentReferencePropertyModel>(new[] { expandedContentReference }));
            _contentApiModel.Properties.Add(DateTimeProp, new DateTimePropertyModel(new EPiServer.Core.PropertyDate(DateTime.Now)));
            _contentApiModel.Properties.Add(BooleanProp, new BooleanPropertyModel(new EPiServer.Core.PropertyBoolean(true)));
            _contentApiModel.Properties.Add(FloatProp, new FloatPropertyModel(new EPiServer.Core.PropertyFloatNumber(1)));

            _contentType = new ContentType { Name = "ContactPage" };
            _contentType.PropertyDefinitions.Add(new PropertyDefinition { Name = ContentAreaProp, LanguageSpecific = false });
            _contentType.PropertyDefinitions.Add(new PropertyDefinition { Name = DateTimeProp, LanguageSpecific = true });
            _contentType.PropertyDefinitions.Add(new PropertyDefinition { Name = BooleanProp, LanguageSpecific = true });
            _contentType.PropertyDefinitions.Add(new PropertyDefinition { Name = FloatProp, LanguageSpecific = true });

            _contentTypeRepository.Setup(x => x.Load(It.IsAny<string>())).Returns(_contentType);
        }

        [Fact]
        public void Filter_WHenRequestIsNotContentManagement_ShouldNotFilterProperties()
        {
            _subject.Filter(_contentApiModel, CreateContext(false));

            Assert.Equal(4, _contentApiModel.Properties.Count);
            _contentTypeRepository.Verify(x => x.Load(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Filter_WhenMasterLanguageIsNull_ShouldNotFilterProperties()
        {
            _contentApiModel.MasterLanguage = null;
            _subject.Filter(_contentApiModel, CreateContext());

            Assert.Equal(4, _contentApiModel.Properties.Count);
            _contentTypeRepository.Verify(x => x.Load(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Filter_WhenContentLanguageIsNull_ShouldNotFilterProperties()
        {
            _contentApiModel.Language = null;
            _subject.Filter(_contentApiModel, CreateContext());

            Assert.Equal(4, _contentApiModel.Properties.Count);
            _contentTypeRepository.Verify(x => x.Load(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Filter_WhenContentIsMasterLanguage_ShouldNotFilterProperties()
        {
            _contentApiModel.MasterLanguage = new LanguageModel { Name = "en" };
            _contentApiModel.Language = new LanguageModel { Name = "en" };
            _subject.Filter(_contentApiModel, CreateContext());

            Assert.Equal(4, _contentApiModel.Properties.Count);
            _contentTypeRepository.Verify(x => x.Load(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Filter_WhenContentTypeIsNotFound_ShouldNotFilterProperties()
        {
            ContentType contentType = default;
            _contentTypeRepository.Setup(x => x.Load(It.IsAny<string>())).Returns(contentType);

            _subject.Filter(_contentApiModel, CreateContext());

            Assert.Equal(4, _contentApiModel.Properties.Count);
            _contentTypeRepository.Verify(x => x.Load(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Filter_WhenRequestIsContentManagement_AndContentIsNotMasterLanguage_ShouldFilterProperties()
        {
            _contentTypeRepository.Setup(x => x.Load(It.IsAny<string>())).Returns(_contentType);

            _subject.Filter(_contentApiModel, CreateContext());

            // ContentAreaProp is removed
            Assert.Equal(3, _contentApiModel.Properties.Count);
            _contentTypeRepository.Verify(x => x.Load(It.IsAny<string>()), Times.Once);
        }

        private ConverterContext CreateContext(bool isManagementRequest = true) =>
           new ConverterContext(null, string.Empty, string.Empty, false, null, ContextMode.Edit, isManagementRequest);
    }
}
