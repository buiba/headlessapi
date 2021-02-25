using System.Collections.Generic;
using Xunit;

namespace EPiServer.ContentManagementApi.Internal
{
    public class ValidateContentTypeAttributeTest
    {
        [Theory]
        [MemberData(nameof(ContentTypeTheoryData))]
        public void IsValid_ContentTypeValidator(object value, bool expected)
        {
            var validateVersionStatus = new ValidateContentTypeAttribute();
            Assert.Equal(expected, validateVersionStatus.IsValid(value));
        }

        public static TheoryData ContentTypeTheoryData => new TheoryData<object, bool>
        {
            { new List<string>() { "ProductPage" }, true },
            { new List<string>(), false },
            { "abc", false },
            { null, false }
        };
    }
}
