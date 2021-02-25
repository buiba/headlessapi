using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.SpecializedProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.ContentApi.Commerce.Internal;
using Xunit;

namespace EPiServer.ContentApi.Commerce.Tests.Serialization.Internal
{
    public class SeoInformationPropertyConverterProviderTest
    {
        [Fact]
        public void Resolve_IfBlockValueIsSeoInformation_ShouldResolve()
        {
            var subject = new SeoInformationPropertyConverterProvider(new SeoInformationPropertyModelConverter());
            var blockProperty = new PropertyBlock<SeoInformation> { Block = new SeoInformation() };
            Assert.NotNull(subject.Resolve(blockProperty));
        }

        [Fact]
        public void Resolve_IfBlockIsOfTypeSeoInformationButValueIsNull_ShouldNotResolve()
        {
            var subject = new SeoInformationPropertyConverterProvider(new SeoInformationPropertyModelConverter());
            var blockProperty = new PropertyBlock<SeoInformation> { Block = null };
            Assert.Null(subject.Resolve(blockProperty));
        }

        [Fact]
        public void Resolve_IfBlockValueIsNotSeoInformation_ShouldNotResolve()
        {
            var subject = new SeoInformationPropertyConverterProvider(new SeoInformationPropertyModelConverter());
            var blockProperty = new PropertyBlock<BlockData> { Block = new BlockData() };
            Assert.Null(subject.Resolve(blockProperty));
        }
    }
}
