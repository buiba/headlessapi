using System.Collections.Generic;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.DataAbstraction;
using Moq;
using Xunit;

namespace EPiServer.ContentManagementApi.Internal
{
    public class ValidateLanguageModelAttributeTest
    {
        [Theory]
        [MemberData(nameof(LanguageModelTheoryData))]
        public void IsValid_LanguageModelValidator(object value, bool expected)
        {
            var languageBranchRepository = new Mock<ILanguageBranchRepository>();
            languageBranchRepository.Setup(l => l.ListEnabled()).Returns(new List<LanguageBranch>() {
                new LanguageBranch("en"),
                new LanguageBranch("en-US")
            });

            var validateLanguageModel = new ValidateLanguageModelAttribute(languageBranchRepository.Object);
            Assert.Equal(expected, validateLanguageModel.IsValid(value));
        }

        [Fact]
        public void IsValid_WhenLanguageIsNotEnabledInCMS_ShouldReturnFalse()
        {
            var languageBranchRepository = new Mock<ILanguageBranchRepository>();
            languageBranchRepository.Setup(l => l.ListEnabled()).Returns(new List<LanguageBranch>() {
                new LanguageBranch("en")                
            });

            var validateLanguageModel = new ValidateLanguageModelAttribute(languageBranchRepository.Object);
            Assert.False(validateLanguageModel.IsValid("en-AE"));
        }

        public static TheoryData LanguageModelTheoryData => new TheoryData<object, bool>
        {
            { null, true},
            { "Test", false},
            { new LanguageModel(), false},
            { new LanguageModel() { Name = "Test" }, false},
            { new LanguageModel() { Name = "en" }, true},
            { new LanguageModel() { Name = "EN" }, true},
            { new LanguageModel() { Name = "en-US" }, true},
        };
    }
}
