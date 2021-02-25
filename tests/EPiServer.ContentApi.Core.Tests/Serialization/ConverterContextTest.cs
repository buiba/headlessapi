using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization
{
    public class ConverterContextTest
    {
        [Fact]
        public void Language_WhenNull_ShouldFallbackToInvariant()
        {
            var context = new ConverterContext(new ContentApiOptions(), string.Empty, string.Empty, false, null);
            Assert.Equal(CultureInfo.InvariantCulture, context.Language);
        }

        [Fact]
        public void ShouldExpand_WhenWildcard_ShouldReturnTrueForAllProperties()
        {
            var context = new ConverterContext(new ContentApiOptions(), string.Empty, "*", false, null);
            Assert.True(context.ShouldExpand("whatever"));
        }

        [Fact]
        public void ShouldExpand_WhenSpecificProperty_ShouldReturnTrueForThatProperty()
        {
            var property = "Body";
            var context = new ConverterContext(new ContentApiOptions(), string.Empty, property, false, null);
            Assert.True(context.ShouldExpand(property));
            Assert.False(context.ShouldExpand("whatever"));
        }
    }
}
