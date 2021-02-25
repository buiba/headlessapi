using System.Collections.Generic;
using Moq;
using Xunit;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    public class DefaultPropertyDataValueConverterResolverTests
    {
        private DefaultPropertyDataValueConverterResolver Subject()
        {
            var provider1 = new Mock<IPropertyDataValueConverterProvider>();
            provider1.Setup(c => c.Resolve(It.IsAny<PropertyModel1>())).Returns(new PropertyDataValueConverterTest());      
            var provider2 = new Mock<IPropertyDataValueConverterProvider>();
            provider2.Setup(c => c.Resolve(It.IsAny<PropertyModel1>())).Returns<IPropertyDataValueConverter>(null);            

            return new DefaultPropertyDataValueConverterResolver(new List<IPropertyDataValueConverterProvider> { provider1.Object, provider2.Object });
        }

        [Fact]
        public void Resolve_IfHasProviders_ShouldReturnConverter()
        {
            var subject = Subject();
            var converter = subject.Resolve(new PropertyModel1());

            Assert.IsType<PropertyDataValueConverterTest>(converter);
        }

        [Fact]
        public void Resolve_IfNotHasProvider_ShouldReturnNull()
        {
            var subject = Subject();
            var converter = subject.Resolve(new PropertyModel2());

            Assert.Null(converter);
        }
    }
}
