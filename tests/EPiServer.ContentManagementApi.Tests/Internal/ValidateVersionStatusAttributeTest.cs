using System;
using EPiServer.Core;
using Xunit;

namespace EPiServer.ContentManagementApi.Internal
{
    public class ValidateVersionStatusAttributeTest
    {
        [Theory]
        [MemberData(nameof(VersionStatusTheoryData))]
        public void IsValid_VersionStatusValidator(object value, bool expected)
        {
            var validateVersionStatus = new ValidateVersionStatusAttribute();
            Assert.Equal(expected, validateVersionStatus.IsValid(value));
        }

        public static TheoryData VersionStatusTheoryData => new TheoryData<object, bool>
        {
            { null, true},
            { new DateTime(), false},
            { VersionStatus.NotCreated, false},
            { 10, false},
            { "NotCreated", false},
            { "TestStatus", false},
            { VersionStatus.Published, true}            
        };
    }
}
