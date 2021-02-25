using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace EPiServer.DefinitionsApi.Manifest.Internal
{
    public class DataAnnotationsValidatorTests
    {
        public DataAnnotationsValidatorTests()
        {
            SaveValidationContextAttribute.SavedContexts.Clear();
        }

        [Fact]
        public void TryValidateObject_on_valid_parent_returns_no_errors()
        {
            var parent = new Parent { PropertyA = 1, PropertyB = 1 };
            var validationResults = new List<ValidationResult>();

            var result = DataAnnotationsValidator.TryValidateObject(parent, validationResults);

            Assert.True(result);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void TryValidateObject_when_missing_required_properties_returns_errors()
        {
            var parent = new Parent { PropertyA = null, PropertyB = null };
            var validationResults = new List<ValidationResult>();

            var result = DataAnnotationsValidator.TryValidateObject(parent, validationResults);

            Assert.False(result);
            Assert.Equal(2, validationResults.Count);
            Assert.Equal(1, validationResults.ToList().Count(x => x.ErrorMessage == "Parent PropertyA is required"));
            Assert.Equal(1, validationResults.ToList().Count(x => x.ErrorMessage == "Parent PropertyB is required"));
        }

        [Fact]
        public void TryValidateObject_calls_IValidatableObject_method()
        {
            var parent = new Parent { PropertyA = 5, PropertyB = 6 };
            var validationResults = new List<ValidationResult>();

            var result = DataAnnotationsValidator.TryValidateObject(parent, validationResults);

            Assert.False(result);
            Assert.Single(validationResults);
            Assert.Equal("Parent PropertyA and PropertyB cannot add up to more than 10", validationResults[0].ErrorMessage);
        }

        [Fact]
        public void TryValidateObjectRecursive_returns_errors_when_child_class_has_invalid_properties()
        {
            var parent = new Parent { PropertyA = 1, PropertyB = 1 };
            parent.Child = new Child { Parent = parent, PropertyA = null, PropertyB = 5 };
            var validationResults = new List<ValidationResult>();

            var result = DataAnnotationsValidator.TryValidateObjectRecursive(parent, validationResults);

            Assert.False(result);
            Assert.Single(validationResults);
            Assert.Equal("Child PropertyA is required", validationResults[0].ErrorMessage);
        }

        [Fact]
        public void TryValidateObjectRecursive_calls_IValidatableObject_method_on_child_class()
        {
            var parent = new Parent { PropertyA = 1, PropertyB = 1 };
            parent.Child = new Child { Parent = parent, PropertyA = 5, PropertyB = 6 };
            var validationResults = new List<ValidationResult>();

            var result = DataAnnotationsValidator.TryValidateObjectRecursive(parent, validationResults);

            Assert.False(result);
            Assert.Single(validationResults);
            Assert.Equal("Child PropertyA and PropertyB cannot add up to more than 10", validationResults[0].ErrorMessage);
        }

        [Fact]
        public void TryValidateObjectRecursive_returns_errors_when_grandchild_class_has_invalid_properties()
        {
            var parent = new Parent { PropertyA = 1, PropertyB = 1 };
            parent.Child = new Child { Parent = parent, PropertyA = 1, PropertyB = 1 };
            parent.Child.GrandChildren = new[] { new GrandChild { PropertyA = 11, PropertyB = 11 } };
            var validationResults = new List<ValidationResult>();

            var result = DataAnnotationsValidator.TryValidateObjectRecursive(parent, validationResults);

            Assert.False(result);
            Assert.Equal(2, validationResults.Count);
            Assert.Equal(1, validationResults.ToList().Count(x => x.ErrorMessage == "GrandChild PropertyA not within range"));
            Assert.Equal(1, validationResults.ToList().Count(x => x.ErrorMessage == "GrandChild PropertyB not within range"));
        }

        [Fact]
        public void TryValidateObjectRecursive_passes_validation_context_items_to_all_validation_calls()
        {
            var parent = new Parent
            {
                Child = new Child
                {
                    GrandChildren = new[] { new GrandChild() }
                }
            };

            var validationResults = new List<ValidationResult>();

            var contextItems = new Dictionary<object, object> { { "key", 12345 } };

            DataAnnotationsValidator.TryValidateObjectRecursive(parent, validationResults, contextItems);

            // Test expects 3 validated properties in the object graph to have a SaveValidationContextAttribute
            Assert.Equal(3, SaveValidationContextAttribute.SavedContexts.Count);
            Assert.True(SaveValidationContextAttribute.SavedContexts.Select(c => c.Items).All(items => items["key"] == contextItems["key"]));
        }

        [Fact]
        public void TryValidateObject_calls_grandchild_IValidatableObject_method()
        {
            var parent = new Parent { PropertyA = 1, PropertyB = 1 };
            parent.Child = new Child { Parent = parent, PropertyA = 1, PropertyB = 1 };
            parent.Child.GrandChildren = new[] { new GrandChild { PropertyA = 5, PropertyB = 6 } };
            var validationResults = new List<ValidationResult>();

            var result = DataAnnotationsValidator.TryValidateObjectRecursive(parent, validationResults);

            Assert.False(result);
            Assert.Single(validationResults);
            Assert.Equal(1, validationResults.ToList().Count(x => x.ErrorMessage == "GrandChild PropertyA and PropertyB cannot add up to more than 10"));
        }

        [Fact]
        public void TryValidateObject_includes_errors_from_all_objects()
        {
            var parent = new Parent { PropertyA = 5, PropertyB = 6 };
            parent.Child = new Child { Parent = parent, PropertyA = 5, PropertyB = 6 };
            parent.Child.GrandChildren = new[] { new GrandChild { PropertyA = 5, PropertyB = 6 } };
            var validationResults = new List<ValidationResult>();

            var result = DataAnnotationsValidator.TryValidateObjectRecursive(parent, validationResults);

            Assert.False(result);
            Assert.Equal(3, validationResults.Count);
            Assert.Equal(1, validationResults.ToList().Count(x => x.ErrorMessage == "Parent PropertyA and PropertyB cannot add up to more than 10"));
            Assert.Equal(1, validationResults.ToList().Count(x => x.ErrorMessage == "Child PropertyA and PropertyB cannot add up to more than 10"));
            Assert.Equal(1, validationResults.ToList().Count(x => x.ErrorMessage == "GrandChild PropertyA and PropertyB cannot add up to more than 10"));
        }

        [Fact]
        public void TryValidateObject_modifies_membernames_for_nested_properties()
        {
            var parent = new Parent { PropertyA = 1, PropertyB = 1 };
            parent.Child = new Child { Parent = parent, PropertyA = null, PropertyB = 5 };
            var validationResults = new List<ValidationResult>();

            var result = DataAnnotationsValidator.TryValidateObjectRecursive(parent, validationResults);

            Assert.False(result);
            Assert.Single(validationResults);
            Assert.Equal("Child PropertyA is required", validationResults[0].ErrorMessage);
            Assert.Equal("Child.PropertyA", validationResults[0].MemberNames.First());
        }

        [Fact]
        public void TryValidateObject_object_with_dictionary_does_not_fail()
        {
            var parent = new Parent { PropertyA = 1, PropertyB = 1 };
            var classWithDictionary = new ClassWithDictionary
            {
                Objects = new List<Dictionary<string, Child>>
                {
                    new Dictionary<string, Child>
                    {
                        {
                            "key",
                            new Child
                            {
                                Parent = parent,
                                PropertyA = 1,
                                PropertyB = 2
                            }
                        }
                    }
                }
            };

            var validationResults = new List<ValidationResult>();

            var result = DataAnnotationsValidator.TryValidateObjectRecursive(classWithDictionary, validationResults);

            Assert.True(result);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void TryValidateObject_object_with_null_enumeration_values_does_not_fail()
        {
            var parent = new Parent { PropertyA = 1, PropertyB = 1 };
            var classWithNullableEnumeration = new ClassWithNullableEnumeration
            {
                Objects = new List<Child>
                {
                    null,
                    new Child
                    {
                        Parent = parent,
                        PropertyA = 1,
                        PropertyB = 2
                    }
                }
            };
            var validationResults = new List<ValidationResult>();

            var result = DataAnnotationsValidator.TryValidateObjectRecursive(classWithNullableEnumeration, validationResults);

            Assert.True(result);
            Assert.Empty(validationResults);
        }

        private class Parent : IValidatableObject
        {
            [Required(ErrorMessage = "Parent PropertyA is required")]
            [Range(0, 10, ErrorMessage = "Parent PropertyA not within range")]
            public int? PropertyA { get; set; }

            [Required(ErrorMessage = "Parent PropertyB is required")]
            [Range(0, 10, ErrorMessage = "Parent PropertyB not within range")]
            public int? PropertyB { get; set; }

            public Child Child { get; set; }

            [SaveValidationContext]
            public bool HasNoRealValidation { get; set; }

            IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
            {
                if (PropertyA.GetValueOrDefault() + PropertyB.GetValueOrDefault() > 10)
                {
                    yield return new ValidationResult("Parent PropertyA and PropertyB cannot add up to more than 10");
                }
            }
        }

        private class Child : IValidatableObject
        {
            [Required(ErrorMessage = "Child Parent is required")]
            public Parent Parent { get; set; }

            [Required(ErrorMessage = "Child PropertyA is required")]
            [Range(0, 10, ErrorMessage = "Child PropertyA not within range")]
            public int? PropertyA { get; set; }

            [Required(ErrorMessage = "Child PropertyB is required")]
            [Range(0, 10, ErrorMessage = "Child PropertyB not within range")]
            public int? PropertyB { get; set; }

            public IEnumerable<GrandChild> GrandChildren { get; set; }

            [SaveValidationContext]
            public bool HasNoRealValidation { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (PropertyA.HasValue && PropertyB.HasValue && (PropertyA + PropertyB > 10))
                {
                    yield return new ValidationResult("Child PropertyA and PropertyB cannot add up to more than 10");
                }
            }
        }

        private class GrandChild : IValidatableObject
        {
            [Required]
            [Range(0, 10, ErrorMessage = "GrandChild PropertyA not within range")]
            public int? PropertyA { get; set; }

            [Required]
            [Range(0, 10, ErrorMessage = "GrandChild PropertyB not within range")]
            public int? PropertyB { get; set; }

            [SaveValidationContext]
            public bool HasNoRealValidation { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (PropertyA.HasValue && PropertyB.HasValue && (PropertyA + PropertyB > 10))
                {
                    yield return new ValidationResult("GrandChild PropertyA and PropertyB cannot add up to more than 10");
                }
            }
        }

        private class ClassWithDictionary
        {
            public List<Dictionary<string, Child>> Objects { get; set; }
        }

        private class ClassWithNullableEnumeration
        {
            public List<Child> Objects { get; set; }
        }

        private class SaveValidationContextAttribute : ValidationAttribute
        {
            public static readonly IList<ValidationContext> SavedContexts = new List<ValidationContext>();

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                SavedContexts.Add(validationContext);
                return ValidationResult.Success;
            }
        }
    }
}
