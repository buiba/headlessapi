using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Xunit;

namespace EPiServer.DefinitionsApi.PropertyDataTypes
{
    public class ExternalPropertyDataTypeTest
    {
        [Fact]
        public void Name_WhenCustomPropertyWithSpace_ShouldBeValid()
        {
            var subject = new ExternalPropertyDataType("String List", null);
            var nameValidator = subject.GetType().GetProperty(nameof(ExternalPropertyDataType.Type)).GetCustomAttribute<RegularExpressionAttribute>();
            Assert.True(nameValidator.IsValid(subject.Type));
        }

        [Fact]
        public void Name_WhenDataTypeStartsWithNumber_ShouldNotBeValid()
        {
            var subject = new ExternalPropertyDataType("1String List", null);
            var nameValidator = subject.GetType().GetProperty(nameof(ExternalPropertyDataType.Type)).GetCustomAttribute<RegularExpressionAttribute>();
            Assert.False(nameValidator.IsValid(subject.Type));
        }
    }
}
