using Xunit;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    public class ValidateVersionAttributeTest
    {
        [Theory]
        [MemberData(nameof(ContentTypeVersionValidatorTheoryData))]
        public void IsValid_ContentTypeVersionValidator(object value, bool expected)
        {
            var validateVersion = new ValidateVersionAttribute();
            Assert.Equal(expected, validateVersion.IsValid(value));
        }

        public static TheoryData ContentTypeVersionValidatorTheoryData => new TheoryData<object, bool>
        {
            { null, true},
            { 1, false},
            { "1", false},
            { "1.0", true},
            { "1.1.0", true},
            { "1.1.1.1", true},
            { "1.1.1.1.1", false},
            { "{test}", false},
            {1.1, false }
        };
    }
}
