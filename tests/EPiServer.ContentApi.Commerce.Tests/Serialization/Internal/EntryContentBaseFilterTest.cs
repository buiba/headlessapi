using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Construction;
using EPiServer.Construction.Internal;
using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.ContentApi.Commerce.Internal;
using Xunit;

namespace EPiServer.ContentApi.Commerce.Tests.Serialization.Internal
{
    public class EntryContentBaseFilterTest
    {
        [Fact]
        public void Filter_EntryContentBase_ShouldRemoveCategoriesParentEntriesAssociations()
        {
            // Arrange
            var content = new ProductContent();
            var propertyDataFactory = new PropertyDataFactory(new ConstructorParameterResolver());
            content.Property.Add("ContentLink", propertyDataFactory.CreateInstance(PropertyDataType.ContentReference));
            content.Property.Add("Categories", propertyDataFactory.CreateInstance(PropertyDataType.Category));
            content.Property.Add("ParentEntries", propertyDataFactory.CreateInstance(PropertyDataType.Block));
            content.Property.Add("Associations", propertyDataFactory.CreateInstance(PropertyDataType.LinkCollection));

            // Act
            var filter = new EntryContentBaseFilter();
            filter.Filter(content, null);

            // Assert
            Assert.Contains("ContentLink", content.Property.Keys);
            Assert.DoesNotContain("Categories", content.Property.Keys);
            Assert.DoesNotContain("ParentEntries", content.Property.Keys);
            Assert.DoesNotContain("Associations", content.Property.Keys);
        }

        [Fact]
        public void Filter_VariationContent_ShouldRemovePriceReferenceAndInventoryReference()
        {
            // Arrange
            var content = new VariationContent()
            {
                Code = "VariantCode1"
            };
            var propertyDataFactory = new PropertyDataFactory(new ConstructorParameterResolver());
            content.Property.Add("PriceReference", propertyDataFactory.CreateInstance(PropertyDataType.ContentReference));
            content.Property.Add("InventoryReference", propertyDataFactory.CreateInstance(PropertyDataType.ContentReference));

            // Act
            var filter = new EntryContentBaseFilter();
            filter.Filter(content, null);

            // Assert
            Assert.False(string.IsNullOrEmpty(content.Code));
            Assert.DoesNotContain("PriceReference", content.Property.Keys);
            Assert.DoesNotContain("InventoryReference", content.Property.Keys);
        }

        [Fact]
        public void Filter_BundleContent_ShouldRemoveBundleReference()
        {
            // Arrange
            var content = new BundleContent()
            {
                Code = "BundleCode1"
            };
            var propertyDataFactory = new PropertyDataFactory(new ConstructorParameterResolver());
            content.Property.Add("BundleReference", propertyDataFactory.CreateInstance(PropertyDataType.ContentReference));

            // Act
            var filter = new EntryContentBaseFilter();
            filter.Filter(content, null);

            // Assert
            Assert.False(string.IsNullOrEmpty(content.Code));
            Assert.DoesNotContain("BundleReference", content.Property.Keys);
        }

        [Fact]
        public void Filter_PackageContent_ShouldRemovePackageReferencePriceReferenceAndInventoryReference()
        {
            // Arrange
            var content = new PackageContent()
            {
                Code = "PackageCode1"
            };
            var propertyDataFactory = new PropertyDataFactory(new ConstructorParameterResolver());
            content.Property.Add("PackageReference", propertyDataFactory.CreateInstance(PropertyDataType.ContentReference));
            content.Property.Add("PriceReference", propertyDataFactory.CreateInstance(PropertyDataType.ContentReference));
            content.Property.Add("InventoryReference", propertyDataFactory.CreateInstance(PropertyDataType.ContentReference));

            // Act
            var filter = new EntryContentBaseFilter();
            filter.Filter(content, null);

            // Assert
            Assert.False(string.IsNullOrEmpty(content.Code));
            Assert.DoesNotContain("PackageReference", content.Property.Keys);
            Assert.DoesNotContain("PriceReference", content.Property.Keys);
            Assert.DoesNotContain("InventoryReference", content.Property.Keys);
        }

        [Fact]
        public void Filter_ProductContent_ShouldRemoveVariantsReference()
        {
            // Arrange
            var content = new ProductContent()
            {
                Code = "ProductCode1"
            };
            var propertyDataFactory = new PropertyDataFactory(new ConstructorParameterResolver());
            content.Property.Add("VariantsReference", propertyDataFactory.CreateInstance(PropertyDataType.ContentReference));

            // Act
            var filter = new EntryContentBaseFilter();
            filter.Filter(content, null);

            // Assert
            Assert.False(string.IsNullOrEmpty(content.Code));
            Assert.DoesNotContain("VariantsReference", content.Property.Keys);
        }

    }
}
