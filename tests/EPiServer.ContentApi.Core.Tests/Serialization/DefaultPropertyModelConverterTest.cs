using System;
using System.Globalization;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.ContentApi.Core.Tests.Serialization.TestSupport;
using EPiServer.Core;
using Moq;
using Xunit;
using System.Collections.Generic;
using EPiServer.SpecializedProperties;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.Core.Serialization;

namespace EPiServer.ContentApi.Core.Tests.Serialization
{
    [Obsolete]
    public class DefaultPropertyModelConverterTest
    {
        private readonly Mock<ReflectionService> _mockActivatorService;
        private readonly DefaultPropertyModelConverter Subject;
        private readonly List<TypeModel> propertyModelMappers;

        public DefaultPropertyModelConverterTest()
        {
            _mockActivatorService = new Mock<ReflectionService>(null);
            _mockActivatorService.Setup(x => x.CreateInstance(It.IsAny<Type>(), It.IsAny<object[]>()))
                .Returns(new Mock<IExpandableProperty>());

            Subject = new DefaultPropertyModelConverter();
            propertyModelMappers = Subject.ModelTypes as List<TypeModel>;
        }

        public class InitializeModelTypes : DefaultPropertyModelConverterTest
        {
            [Fact]
            public void It_should_able_to_handle_PropertyAppSettings()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyAppSettings) && m.ModelType == typeof(AppSettingsPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyAppSettingsMultiple()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyAppSettingsMultiple) && m.ModelType == typeof(AppSettingsMultiplePropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyBlob()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyBlob) && m.ModelType == typeof(BlobPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyBoolean()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyBoolean) && m.ModelType == typeof(BooleanPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyCategory()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyCategory) && m.ModelType == typeof(CategoryPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyCheckBoxList()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyCheckBoxList) && m.ModelType == typeof(CheckboxListPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyContentArea()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyContentArea) && m.ModelType == typeof(ContentAreaPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyContentReference()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyContentReference) && m.ModelType == typeof(ContentReferencePropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyPageReference()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyPageReference) && m.ModelType == typeof(PageReferencePropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyContentReferenceList()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyContentReferenceList) && m.ModelType == typeof(ContentReferenceListPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyDate()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyDate) && m.ModelType == typeof(DateTimePropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyDocumentUrl()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyDocumentUrl) && m.ModelType == typeof(DocumentUrlPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyDropDownList()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyDropDownList) && m.ModelType == typeof(DropDownListPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyFileSortOrder()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyFileSortOrder) && m.ModelType == typeof(FileSortOrderPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyFloatNumber()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyFloatNumber) && m.ModelType == typeof(FloatPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyFrame()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyFrame) && m.ModelType == typeof(FramePropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyImageUrl()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyImageUrl) && m.ModelType == typeof(ImageUrlPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyLanguage()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyLanguage) && m.ModelType == typeof(LanguagePropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyLinkCollection()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyLinkCollection) && m.ModelType == typeof(LinkCollectionPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyLongString()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyLongString) && m.ModelType == typeof(LongStringPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyNumber()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyNumber) && m.ModelType == typeof(NumberPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyPageType()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyPageType) && m.ModelType == typeof(PageTypePropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertySelector()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertySelector) && m.ModelType == typeof(SelectorPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertySortOrder()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertySortOrder) && m.ModelType == typeof(SortOrderPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyString()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyString) && m.ModelType == typeof(StringPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyUrl()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyUrl) && m.ModelType == typeof(UrlPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyVirtualLink()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyVirtualLink) && m.ModelType == typeof(VirtualLinkPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyWeekDay()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyWeekDay) && m.ModelType == typeof(WeekdayPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyXhtmlString()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyXhtmlString) && m.ModelType == typeof(XHtmlPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyStringList()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyStringList) && m.ModelType == typeof(StringListPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyIntegerList()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyIntegerList) && m.ModelType == typeof(IntegerListPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyDoubleList()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyDoubleList) && m.ModelType == typeof(DoubleListPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_PropertyDateList()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(PropertyDateList) && m.ModelType == typeof(DateListPropertyModel));
            }

            [Fact]
            public void It_should_able_to_handle_custom_property_data()
            {
                Assert.Contains(propertyModelMappers, m => m.PropertyType == typeof(CustomPropertyData) && m.ModelType == typeof(CustomPropertyModel));
            }
        }

        public class When_property_model_implement_IExcludeFromDefaultModelTypeRegistration : DefaultPropertyModelConverterTest
        {
            internal class CustomContentAreaPropertyModel : ContentAreaPropertyModel, IExcludeFromModelTypeRegistration
            {
                public CustomContentAreaPropertyModel(PropertyContentArea propertyContentArea, bool excludePersonalizedContent)
                                                : base(propertyContentArea, excludePersonalizedContent)
                {

                }
            }

            [Fact]
            public void It_should_not_contain_custom_content_area_property_model()
            {
                Assert.DoesNotContain(propertyModelMappers, m => m.PropertyType == typeof(PropertyContentArea) && m.ModelType == typeof(CustomContentAreaPropertyModel));
            }
        }

        public class GetValue : DefaultPropertyModelConverterTest
        {

            public class WhenPropertyIsNul : GetValue
            {
                [Fact]
                public void ItShouldReturnNull()
                {
                    var defaultPropertyModelHandler = new DefaultPropertyModelConverter(_mockActivatorService.Object);
                    var result = defaultPropertyModelHandler.ConvertToPropertyModel(null, new CultureInfo("en"), false, false);
                    Assert.Null(result);
                }
            }

            public class WhenPropertyIsNotNullAndIsMapped : GetValue
            {
                [Fact]
                public void ItShouldReturnCorrectPropertyModelOfGivenProperty()
                {
                    var stringPropertyData = new PropertyString();
                    var defaultPropertyModelHandler = new DefaultPropertyModelConverter(new PropertyModelFactory())
                    {
                        ModelTypes = new[] { new TypeModel { ModelType = typeof(StringPropertyModel), PropertyType = typeof(PropertyString) } },
                        Config = new Configuration.ContentApiConfiguration()
                    };

                    var result = defaultPropertyModelHandler.ConvertToPropertyModel(stringPropertyData, new CultureInfo("en"), false, false);
                    Assert.IsType<StringPropertyModel>(result);
                }
            }

            public class WhenPropertyIsNotNullAndNotMapped : GetValue
            {
                [Fact]
                public void ItShouldReturnNull()
                {
                    var defaultPropertyModelHandler = new DefaultPropertyModelConverter(_mockActivatorService.Object);
                    var testPropertyData = new TestPropertyData();
                    var result = defaultPropertyModelHandler.ConvertToPropertyModel(testPropertyData, new CultureInfo("en"), false, false);
                    Assert.Null(result);
                }
            }

            public class WhenPropertyIsTypeOfIPersonalizableProperty : GetValue
            {
                [Fact]
                public void ItShouldReturnPersonalizableModel()
                {
                    var propertyData = new PropertyContentReference() { Name = "something" };

                    var defaultPropertyModelHandler = new DefaultPropertyModelConverter(new PropertyModelFactory())
                    {
                        ModelTypes = new[] { new TypeModel { ModelType = typeof(PersonalizableContentReferenceModel), PropertyType = typeof(PropertyContentReference) } },
                        Config = new Configuration.ContentApiConfiguration()
                    };

                    var model = defaultPropertyModelHandler.ConvertToPropertyModel(propertyData, It.IsAny<CultureInfo>(), true, false);
                    Assert.True((model as PersonalizableContentReferenceModel).ExcludePersonalizedContent);
                }
            }

            public class WhenPropertyIsExpandableAndExpandSetToTrue : GetValue
            {
                [Fact]
                public void ItShouldCallExpandMethodOfPropertyModel()
                {
                    var propertyData = new PropertyContentReference() { Name = "something" };

                    var defaultPropertyModelHandler = new DefaultPropertyModelConverter(new PropertyModelFactory())
                    {
                        ModelTypes = new[] { new TypeModel { ModelType = typeof(ExpandablePropertyModel), PropertyType = typeof(PropertyContentReference) } },
                        Config = new Configuration.ContentApiConfiguration()
                    };

                    var model = defaultPropertyModelHandler.ConvertToPropertyModel(propertyData, It.IsAny<CultureInfo>(), false, true);
                    Assert.True((model as ExpandablePropertyModel).ExpandCalled);
                }
            }

            public class WhenPropertyIsExpandableAndExpandSetToFalse : GetValue
            {
                [Fact]
                public void ItShouldNotCallExpandMethodOfPropertyModel()
                {
                    var propertyData = new PropertyContentReference() { Name = "something" };

                    var defaultPropertyModelHandler = new DefaultPropertyModelConverter(new PropertyModelFactory())
                    {
                        ModelTypes = new[] { new TypeModel { ModelType = typeof(ExpandablePropertyModel), PropertyType = typeof(PropertyContentReference) } },
                        Config = new Configuration.ContentApiConfiguration()
                    };

                    var model = defaultPropertyModelHandler.ConvertToPropertyModel(propertyData, It.IsAny<CultureInfo>(), false, false);
                    Assert.False((model as ExpandablePropertyModel).ExpandCalled);
                }
            }
        }

        public class CanHandleProperty : DefaultPropertyModelConverterTest
        {
            public class WhenPropertyIsNull : CanHandleProperty
            {
                [Fact]
                public void ItShouldReturnFalse()
                {
                    var defaultPropertyModelHandler = new DefaultPropertyModelConverter();
                    Assert.False(defaultPropertyModelHandler.HasPropertyModelAssociatedWith(null));
                }
            }

            public class WhenPropertyIsNotNullAndIsMapped : CanHandleProperty
            {
                [Fact]
                public void ItShouldReturnTrue()
                {
                    var defaultPropertyModelHandler = new DefaultPropertyModelConverter();
                    var stringPropertyData = new PropertyString();
                    Assert.True(defaultPropertyModelHandler.HasPropertyModelAssociatedWith(stringPropertyData));
                }
            }

            public class WhenPropertyIsNotNullAndNotMapped : CanHandleProperty
            {
                [Fact]
                public void ItShouldReturnTrue()
                {
                    var defaultPropertyModelHandler = new DefaultPropertyModelConverter();
                    //Unregistered instance of PropertyData
                    var testPropertyData = new TestPropertyData();
                    Assert.False(defaultPropertyModelHandler.HasPropertyModelAssociatedWith(testPropertyData));
                }
            }
        }

        public class ExpandablePropertyModel : IPropertyModel, IExpandableProperty
        {
            public string Name { get; set; }

            public string PropertyDataType { get; }
            public bool ExpandCalled { get; private set; }

            public void Expand(CultureInfo language)
            {
                ExpandCalled = true;
            }
        }

        public class PersonalizableContentReferenceModel : PersonalizablePropertyModel<ContentApiModel, PropertyContentReference>
        {
            public PersonalizableContentReferenceModel(PropertyContentReference propertyData, bool excludePersonalizedContent) : base(propertyData, excludePersonalizedContent)
            {
            }
        }

    }
}
