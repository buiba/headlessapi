using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentManagementApi.Serialization.Internal.Converters;
using Moq;
using Xunit;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    public class PropertyBlockValueConverterProviderTests
    {
        private PropertyBlockValueConverterProvider Subject()
        {
            var subject = new PropertyBlockValueConverterProvider();
            subject.Initialize(Mock.Of<IPropertyDataValueConverterResolver>());
            return subject;
        }

        [Fact]
        public void Resolve_IfPropertyModelIsNull_ShouldReturnNull()
        {
            var subject = Subject();
            var converter = subject.Resolve(null);

            Assert.Null(converter);
        }

        [Fact]
        public void Resolve_IfTheResolverIsNull_ShouldReturnNull()
        {
            var subject = new PropertyBlockValueConverterProvider();
            var converter = subject.Resolve(new StringPropertyModel());

            Assert.Null(converter);
        }

        [Fact]
        public void Resolve_IfPropertyModelIsNotBlockPropertyModel_ShouldReturnNull()
        {
            var subject = Subject();
            var converter = subject.Resolve(new StringPropertyModel());

            Assert.Null(converter);
        }        

        [Fact]
        public void Resolve_IfPropertyModelIsBlockPropertyModel_ShouldReturnPropertyBlockValueConverter()
        {
            var subject = Subject();
            var converter = subject.Resolve(new BlockPropertyModel());

            Assert.IsType<PropertyBlockValueConverter>(converter);
        }                
    }
}
