using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentManagementApi.Serialization.Internal.Converters;
using EPiServer.Core;
using FluentAssertions;
using Xunit;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    public class PropertyCategoryValueConverterTests
    {
        private PropertyCategoryValueConverter Subject() => new PropertyCategoryValueConverter();

        [Fact]
        public void Convert_IfModelIsNull_ShouldThrowArgumentNullException()
        {
            var subject = Subject();

            Assert.Throws<ArgumentNullException>(() => subject.Convert(null, null));
        }

        [Fact]
        public void Convert_IfModelIsNotCategoryPropertyModel_ShouldThrowNotSupportedException()
        {
            var subject = Subject();

            Assert.Throws<NotSupportedException>(() => subject.Convert(new BlockPropertyModel(), null));
        }

        [Fact]
        public void Convert_IfCategoryPropertyModel_ShouldReturnCategoryList()
        {
            var subject = Subject();
            var categoryModels = new List<CategoryModel>
            {
                new CategoryModel()
                    {
                        Id = 1,
                        Name = "Category 1",
                        Description = "Category 1 descriotion"
                    },
                new CategoryModel()
                    {
                        Id = 2,
                        Name = "Category 2",
                        Description = "Category 2 descriotion"
                    },
            };

            var propertyModel = new CategoryPropertyModel
            {
                Value = categoryModels
            };

            var categoryList = subject.Convert(propertyModel, null) as CategoryList;

            categoryList.Should().BeEquivalentTo(categoryModels.Select(x => x.Id));
        }

        [Fact]
        public void Convert_IfModelNotHasValue_ShouldReturnNull()
        {
            var subject = Subject();

            var propertyModel = new CategoryPropertyModel
            {
                Value = null
            };

            var categoryList = subject.Convert(propertyModel, null) as CategoryList;

            categoryList.Should().BeNull();
        }
    }
}
