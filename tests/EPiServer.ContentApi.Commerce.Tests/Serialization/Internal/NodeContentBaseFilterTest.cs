using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Construction;
using EPiServer.Construction.Internal;
using EPiServer.ContentApi.Commerce.Internal;
using EPiServer.Core;
using Xunit;

namespace EPiServer.ContentApi.Commerce.Tests.Serialization.Internal
{
    public class NodeContentBaseFilterTest
    {
        [Fact]
        public void Filter_NodeContentBase_ShouldRemoveCategories()
        {
            // Arrange
            var content = new NodeContent();
            var propertyDataFactory = new PropertyDataFactory(new ConstructorParameterResolver());
            content.Property.Add("Categories", propertyDataFactory.CreateInstance(PropertyDataType.Category));

            // Act
            var filter = new NodeContentBaseFilter();
            filter.Filter(content, null);

            // Assert
            Assert.DoesNotContain("Categories", content.Property.Keys);
        }
    }
}
