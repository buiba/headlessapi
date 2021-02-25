using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ContentApi.Commerce.Internal;
using EPiServer.Core;
using EPiServer.SpecializedProperties;
using Xunit;

namespace EPiServer.ContentApi.Commerce.Tests.Serialization
{
    public class SeoInformationPropertyModelConverterTest
    {
        private readonly SeoInformationPropertyModelConverter Subject;

        public SeoInformationPropertyModelConverterTest()
        {
            Subject = new SeoInformationPropertyModelConverter();
        }

        public class HasPropertyModelAssociatedWith : SeoInformationPropertyModelConverterTest
        {
            [Fact]
            public void ShouldReturnTrue_WhenPropertyDataIsSeoInformation()
            {
                var propertyData = new PropertyBlock<SeoInformation>()
                {
                    Value = new SeoInformation()
                };

                Assert.True(Subject.HasPropertyModelAssociatedWith(propertyData));
            }

            [Fact]
            public void ShouldReturnFalse_WhenPropertyDataIsNotSeoInformation()
            {
                var propertyData = new PropertyBlock<BlockData>()
                {
                    Value = new BlockData()
                };

                Assert.False(Subject.HasPropertyModelAssociatedWith(propertyData));
            }
        }

        public class ConvertToPropertyModel: SeoInformationPropertyModelConverterTest
        {
            [Fact]
            public void ShouldReturnValue_WhenPropertyDataValueIsSeoInformation()
            {
                var propertyData = new PropertyBlock<BlockData>()
                {
                    Value = new SeoInformation()
                };

                Assert.NotNull(Subject.ConvertToPropertyModel(propertyData, null, false, false));
            }
        }
    }
}
