using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.ContentTypes.Internal;
using EPiServer.Commerce.Catalog.Provider;
using EPiServer.Commerce.Catalog.Provider.Construction;
using EPiServer.Commerce.Internal;
using EPiServer.Commerce.Marketing.Internal;
using EPiServer.DefinitionsApi.Commerce.Internal;
using EPiServer.DataAbstraction;
using EPiServer.DataAbstraction.RuntimeModel;
using EPiServer.DefinitionsApi.ContentTypes;
using EPiServer.DefinitionsApi.ContentTypes.Internal;
using Mediachase.BusinessFoundation.Data.Meta.Management;
using Moq;
using Xunit;

namespace EPiServer.ContentManagement.ContentTypesApi.Commerce.Tests.Internal
{
    public class CommerceExternalContentTypeServiceExtensionTest
    {
        [Fact]
        public void TryDelete_WhenNoMatchingContentTypeExists_ShouldReturnFalse()
        {
            Assert.False(Subject().TryDelete(Guid.NewGuid()));
        }

        [Fact]
        public void TryDelete_WhenNotCommerceContentType_ShouldReturnFalse()
        {
            var existing = new PageType { GUID = Guid.NewGuid(), Name = "One" };
            var subject = Subject(existing);

            var result = subject.TryDelete(existing.GUID);

            Assert.False(result);
        }

        [Fact]
        public void TryDelete_WhenMarketingContentTypeExists_ShouldCallDeleteOnInnerRepository()
        {
            var existing = new ContentType { GUID = Guid.NewGuid(), Name = "One", Base = MarketingContentTypeBase.SalesCampaign };
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Load(existing.GUID)).Returns(existing);

            var subject = Subject(inner.Object);

            var result = subject.TryDelete(existing.GUID);

            Assert.True(result);
            inner.Verify(x => x.Delete(existing), Times.Once());
        }

        [Fact]
        public void TryDelete_WhenCatalogContentTypeExists_ShouldCallDeleteOnInnerRepositoryAndDeleteMetaClass()
        {
            var existing = new ContentType { GUID = Guid.NewGuid(), Name = "One", Base = CatalogContentTypeBase.Variation };
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.Load(existing.GUID)).Returns(existing);

            var subject = new Mock<CommerceExternalContentTypeServiceExtension>(inner.Object);

            var result = subject.Object.TryDelete(existing.GUID);

            Assert.True(result);
            inner.Verify(x => x.Delete(existing), Times.Once());
            subject.Verify(x => x.DeleteMetaClass(existing), Times.Once());
        }

        [Fact]
        public void Save_WhenNotCommerceContentTypes_ShouldNotCallSaveCommerceContentTypes()
        {
            var externalContentTypes = new ExternalContentType[]
            {
                new ExternalContentType { Id = Guid.NewGuid(), Name = "One", BaseType = ContentTypeBase.Page.ToString() },
                new ExternalContentType { Id = Guid.NewGuid(), Name = "Two", BaseType = ContentTypeBase.Block.ToString() },
                new ExternalContentType { Id = Guid.NewGuid(), Name = "Three", BaseType = ContentTypeBase.Media.ToString() }
            };
            var internalContentTypes = externalContentTypes.Select(c => new ContentType
            {
                GUID = c.Id,
                Name = c.Name,
                Base = new ContentTypeBase(c.BaseType)
            });
            var contentTypeSaveOptions = new ContentTypeSaveOptions
            {
                AllowedUpgrades = VersionComponent.Major,
                AllowedDowngrades = VersionComponent.Minor,
                AutoIncrementVersion = true
            };

            var inner = new Mock<ContentTypeRepository>();
            var subject = new Mock<CommerceExternalContentTypeServiceExtension>(inner.Object);

            subject.Object.Save(externalContentTypes, internalContentTypes, contentTypeSaveOptions);

            subject.Verify(x => x.SaveCommerceContentTypes(internalContentTypes, contentTypeSaveOptions), Times.Never());
        }

        [Fact]
        public void Save_WhenCommerceContentTypes_ShouldCallSaveCommerceContentTypes_WithCommerceContentTypesAsArguments()
        {
            var commerceExternalContentTypes = new ExternalContentType[]
            {
                new ExternalContentType { Id = Guid.NewGuid(), Name = "Com One", BaseType = CatalogContentTypeBase.Product.ToString() },
                new ExternalContentType { Id = Guid.NewGuid(), Name = "Com Two", BaseType = CatalogContentTypeBase.Variation.ToString() },
                new ExternalContentType { Id = Guid.NewGuid(), Name = "Com Three", BaseType = MarketingContentTypeBase.Promotion.ToString() },
            };

            var externalContentTypes = new List<ExternalContentType>()
            {
                new ExternalContentType { Id = Guid.NewGuid(), Name = "One", BaseType = ContentTypeBase.Page.ToString() },
                new ExternalContentType { Id = Guid.NewGuid(), Name = "Two", BaseType = ContentTypeBase.Block.ToString() },
                new ExternalContentType { Id = Guid.NewGuid(), Name = "Three", BaseType = ContentTypeBase.Media.ToString() },
            };
            externalContentTypes.AddRange(commerceExternalContentTypes);

            var internalContentTypes = externalContentTypes.Select(c => new ContentType
            {
                GUID = c.Id,
                Name = c.Name,
                Base = new ContentTypeBase(c.BaseType)
            });
            var commerceInternalContentTypes = internalContentTypes.Where(x => commerceExternalContentTypes.Any(e => e.Id == x.GUID));

            var contentTypeSaveOptions = new ContentTypeSaveOptions
            {
                AllowedUpgrades = VersionComponent.Major,
                AllowedDowngrades = VersionComponent.Minor,
                AutoIncrementVersion = true
            };

            var inner = new Mock<ContentTypeRepository>();
            var subject = new Mock<CommerceExternalContentTypeServiceExtension>(inner.Object);

            subject.Object.Save(externalContentTypes, internalContentTypes, contentTypeSaveOptions);

            subject.Verify(x => x.SaveCommerceContentTypes(commerceInternalContentTypes, contentTypeSaveOptions), Times.Once());
        }

        [Fact]
        public void Save_WhenMarketingContentTypes_ShouldCallSaveOnInternalRepository()
        {
            var externalContentTypes = new ExternalContentType[]
            {
                new ExternalContentType { Id = Guid.NewGuid(), Name = "One", BaseType = MarketingContentTypeBase.SalesCampaign.ToString() }
            };
            var internalContentTypes = externalContentTypes.Select(c => new ContentType
            {
                GUID = c.Id,
                Name = c.Name,
                Base = new ContentTypeBase(c.BaseType)
            });
            var contentTypeSaveOptions = new ContentTypeSaveOptions
            {
                AllowedUpgrades = VersionComponent.Major,
                AllowedDowngrades = VersionComponent.Minor,
                AutoIncrementVersion = true
            };

            var inner = new Mock<ContentTypeRepository>();
            var subject = new Mock<CommerceExternalContentTypeServiceExtension>(inner.Object);
            subject.Setup(x => x.SaveCommerceContentTypes(It.IsAny<IEnumerable<ContentType>>(), It.IsAny<ContentTypeSaveOptions>())).CallBase();

            subject.Object.Save(externalContentTypes, internalContentTypes, contentTypeSaveOptions);

            inner.Verify(x => x.Save(internalContentTypes, contentTypeSaveOptions), Times.Once());
            subject.Verify(x => x.SaveMetaClass(internalContentTypes.Single()), Times.Never());
            subject.Verify(x => x.SaveMetaField(internalContentTypes.Single()), Times.Never());
        }

        [Fact]
        public void Save_WhenCatalogContentTypes_ShouldCallSaveOnInternalRepositoryAndSaveMetaDataApi()
        {
            var externalContentTypes = new ExternalContentType[]
            {
                new ExternalContentType { Id = Guid.NewGuid(), Name = "One", BaseType = CatalogContentTypeBase.Product.ToString() }
            };
            var internalContentTypes = externalContentTypes.Select(c => new ContentType
            {
                GUID = c.Id,
                Name = c.Name,
                Base = new ContentTypeBase(c.BaseType)
            });
            var contentTypeSaveOptions = new ContentTypeSaveOptions
            {
                AllowedUpgrades = VersionComponent.Major,
                AllowedDowngrades = VersionComponent.Minor,
                AutoIncrementVersion = true
            };

            var inner = new Mock<ContentTypeRepository>();
            var subject = new Mock<CommerceExternalContentTypeServiceExtension>(inner.Object);
            subject.Setup(x => x.SaveCommerceContentTypes(It.IsAny<IEnumerable<ContentType>>(), It.IsAny<ContentTypeSaveOptions>())).CallBase();

            subject.Object.Save(externalContentTypes, internalContentTypes, contentTypeSaveOptions);

            inner.Verify(x => x.Save(internalContentTypes, contentTypeSaveOptions), Times.Once());
            subject.Verify(x => x.SaveMetaClass(internalContentTypes.Single()), Times.Once());
            subject.Verify(x => x.SaveMetaField(internalContentTypes.Single()), Times.Once());
        }


        private static CommerceExternalContentTypeServiceExtension Subject(params ContentType[] contentTypes)
        {
            var inner = new Mock<ContentTypeRepository>();
            inner.Setup(x => x.List()).Returns(contentTypes);
            inner.Setup(x => x.Load(It.IsAny<Guid>())).Returns<Guid>(id => contentTypes.FirstOrDefault(x => x.GUID == id));

            return Subject(inner.Object);
        }

        private static CommerceExternalContentTypeServiceExtension Subject(ContentTypeRepository internalRepository = null)
          => new CommerceExternalContentTypeServiceExtension(
                internalRepository ?? Mock.Of<ContentTypeRepository>(),
                new CommerceContentTypeBaseProvider(),
                Mock.Of<IContentTypeModelAssigner>(),
                Mock.Of<MetaDataTypeResolver>(),
                new Mock<MetaDataPropertyResolver>(new MetaDataPropertyMapper()).Object,
                Mock.Of<IPropertyDefinitionTypeRepository>()
            );

    }
}
