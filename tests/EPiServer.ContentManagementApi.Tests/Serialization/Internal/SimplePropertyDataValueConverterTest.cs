using System;
using System.Collections.Generic;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentManagementApi.Serialization.Internal.Converters;
using EPiServer.Core;
using EPiServer.Filters;
using EPiServer.SpecializedProperties;
using EPiServer.Web.PropertyControls;
using Xunit;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    public class SimplePropertyDataValueConverterTest
    {
        [Fact]
        public void Convert_WithNullPropertyModel_ShouldThrow()
        {   
            Assert.Throws<ArgumentNullException>(() => new SimplePropertyDataValueConverter().Convert(null, null));
        }

        [Fact]
        public void Convert_WithUnsupportedPropertyModel_ShouldThrow()
        {
            Assert.Throws<NotImplementedException>(() => new SimplePropertyDataValueConverter().Convert(new PropertyModel1(), null));
        }

        [Theory]
        [MemberData(nameof(PropertyTheoryData))]
        public void Convert_WithRegisteredPropertyModel_ShouldCreateCorrectPropertyData(IPropertyModel propertyModel, object value)
        {   
            var result = new SimplePropertyDataValueConverter().Convert(propertyModel, null);
            
            Assert.Equal(value, result);
        }

        [Fact]
        public void Convert_WithNumberPropertyModel_ShouldCreateCorrectPropertyData()
        {
            var expectedPropertyData = new PropertyNumber()
            {
                Number = 1
            };
            var propertyModel = new NumberPropertyModel(expectedPropertyData);
            var result = new SimplePropertyDataValueConverter().Convert(propertyModel, null);
                        
            Assert.Equal(expectedPropertyData.Value, result);
            Assert.Equal(expectedPropertyData.Number, result);
        }

        [Fact]
        public void Convert_WithFileSortOrderPropertyModel_ShouldCreateCorrectPropertyData()
        {
            var expectedPropertyData = new PropertyFileSortOrder()
            {
                SortOrder = FileSortOrder.Name
            };
            var propertyModel = new FileSortOrderPropertyModel(expectedPropertyData);
            var result = new SimplePropertyDataValueConverter().Convert(propertyModel, null);
            
            Assert.Equal(expectedPropertyData.Value, result);
            Assert.Equal(expectedPropertyData.Number, result);
            Assert.Equal(expectedPropertyData.SortOrder, (FileSortOrder)result);
        }

        [Fact]
        public void Convert_WithFloatNumberPropertyModel_ShouldCreateCorrectPropertyData()
        {
            var expectedPropertyData = new PropertyFloatNumber()
            {
                FloatNumber = 1f
            };
            var propertyModel = new FloatPropertyModel(expectedPropertyData);
            var result = new SimplePropertyDataValueConverter().Convert(propertyModel, null);
            
            Assert.Equal(expectedPropertyData.Value, result);
            Assert.Equal(expectedPropertyData.FloatNumber, result);
        }

        [Fact]
        public void Convert_WithSortOrderPropertyModel_ShouldCreateCorrectPropertyData()
        {
            var expectedPropertyData = new PropertySortOrder()
            {
                SortOrder = FilterSortOrder.Alphabetical
            };
            var propertyModel = new SortOrderPropertyModel(expectedPropertyData);
            var result = new SimplePropertyDataValueConverter().Convert(propertyModel, null);
            
            Assert.Equal(expectedPropertyData.Value, result);
            Assert.Equal(expectedPropertyData.Number, (int)result);
            Assert.Equal(expectedPropertyData.SortOrder, (FilterSortOrder)result);
        }

        [Fact]
        public void Convert_WithBooleanPropertyModel_ShouldCreateCorrectPropertyData()
        {
            var expectedPropertyData = new PropertyBoolean()
            {
                Boolean = true
            };
            var propertyModel = new BooleanPropertyModel(expectedPropertyData);
            var result = new SimplePropertyDataValueConverter().Convert(propertyModel, null);
            
            Assert.Equal(expectedPropertyData.Value, result);
            Assert.Equal(expectedPropertyData.Boolean, result);
        }

        [Fact]
        public void Convert_WithDatePropertyModel_ShouldCreateCorrectPropertyData()
        {
            var expectedPropertyData = new PropertyDate()
            {
                Date = DateTime.Now.ToUniversalTime()
            };
            var propertyModel = new DateTimePropertyModel(expectedPropertyData);
            var result = new SimplePropertyDataValueConverter().Convert(propertyModel, null);
            
            Assert.Equal(expectedPropertyData.Value, result);
            Assert.Equal(expectedPropertyData.Date, result);
        }

        [Fact]
        public void Convert_WithDoubleListPropertyModel_ShouldCreateCorrectPropertyData()
        {
            var expectedPropertyData = new PropertyDoubleList()
            {
                List = new List<double> { 1f, 2f }
            };
            var propertyModel = new DoubleListPropertyModel(expectedPropertyData);
            var result = new SimplePropertyDataValueConverter().Convert(propertyModel, null);
            
            Assert.Equal(expectedPropertyData.Value, result);
            Assert.Equal(expectedPropertyData.List, result);
        }

        [Fact]
        public void Convert_WithIntegerListPropertyModel_ShouldCreateCorrectPropertyData()
        {
            var expectedPropertyData = new PropertyIntegerList()
            {
                List = new List<int> { 1, 2 }
            };
            var propertyModel = new IntegerListPropertyModel(expectedPropertyData);
            var result = new SimplePropertyDataValueConverter().Convert(propertyModel, null);
            
            Assert.Equal(expectedPropertyData.Value, result);
            Assert.Equal(expectedPropertyData.List, result);
        }

        [Fact]
        public void Convert_WithPageTypePropertyModel_ShouldCreateCorrectPropertyData()
        {
            var expectedPropertyData = new PropertyPageType()
            {
                PageTypeName = "PageName"
            };
            var propertyModel = new PageTypePropertyModel(expectedPropertyData);
            var result = new SimplePropertyDataValueConverter().Convert(propertyModel, null);            
           
            Assert.Equal(expectedPropertyData.PageTypeName, result);
        }

        [Fact]
        public void Convert_WithStringListPropertyModel_ShouldCreateCorrectPropertyData()
        {
            var expectedPropertyData = new PropertyStringList()
            {
                List = new List<string> { "Item1", "Item2" }
            };
            var propertyModel = new StringListPropertyModel(expectedPropertyData);
            var result = new SimplePropertyDataValueConverter().Convert(propertyModel, null);
            
            Assert.Equal(expectedPropertyData.Value, result);
            Assert.Equal(expectedPropertyData.List, result);
        }

        [Fact]
        public void Convert_WithFramePropertyModel_ShouldCreateCorrectPropertyData()
        {
            var expectedPropertyData = new PropertyFrame()
            {
                Value = 1
            };
            var propertyModel = new FramePropertyModel(expectedPropertyData);
            var result = new SimplePropertyDataValueConverter().Convert(propertyModel, null);

            Assert.Equal(expectedPropertyData.Value, Convert.ToInt32(result));            
        }

        [Fact]
        public void Convert_WithWeekdayPropertyModel_ShouldCreateCorrectPropertyData()
        {
            var expectedPropertyData = new PropertyWeekDay()
            {
                Value = "Monday"
            };
            var propertyModel = new WeekdayPropertyModel(expectedPropertyData);
            var result = new SimplePropertyDataValueConverter().Convert(propertyModel, null);

            Enum.TryParse<Weekday>((string)result, true, out var parsedWeekday);
            Assert.Equal(expectedPropertyData.Value, parsedWeekday);
        }

        public static TheoryData PropertyTheoryData => new TheoryData<IPropertyModel, object>
        {
            { new StringPropertyModel(TestPropertyString), TestPropertyString.Value},
            { new LongStringPropertyModel(TestPropertyLongString), TestPropertyLongString.Value},
            { new AppSettingsPropertyModel(TestPropertyAppSettings), TestPropertyAppSettings.Value},
            { new BlobPropertyModel(TestPropertyBlob), TestPropertyBlob.Value},
            { new DocumentUrlPropertyModel(TestPropertyDocumentUrl), TestPropertyDocumentUrl.Value},
            { new DropDownListPropertyModel(TestPropertyDropDownList), TestPropertyDropDownList.Value},
            { new ImageUrlPropertyModel(TestPropertyImageUrl), TestPropertyImageUrl.Value},
            { new LanguagePropertyModel(TestPropertyLanguage), TestPropertyLanguage.Value},
            { new UrlPropertyModel(TestPropertyUrl), TestPropertyUrl.Value},
            { new VirtualLinkPropertyModel(TestPropertyVirtualLink), TestPropertyVirtualLink.Value},            
            { new CheckboxListPropertyModel(TestPropertyCheckBoxList), TestPropertyCheckBoxList.Value},
            { new DateListPropertyModel(TestPropertyDateList), TestPropertyDateList.Value},
            { new SelectorPropertyModel(TestPropertySelector), TestPropertySelector.Value},
            { new AppSettingsMultiplePropertyModel(TestPropertyAppSettingsMultiple), TestPropertyAppSettingsMultiple.Value}
        };

        private static readonly PropertyString TestPropertyString = new PropertyString()
        {
            Value = "Test"
        };

        private static readonly PropertyLongString TestPropertyLongString = new PropertyLongString()
        {
            Value = "Test"
        };

        private static readonly PropertyAppSettings TestPropertyAppSettings = new PropertyAppSettings()
        {
            Value = "Test"
        };

        private static readonly PropertyBlob TestPropertyBlob = new PropertyBlob();

        private static readonly PropertyDocumentUrl TestPropertyDocumentUrl = new PropertyDocumentUrl();

        private static readonly PropertyDropDownList TestPropertyDropDownList = new PropertyDropDownList()
        {
            Value = "Test"
        };

        private static readonly PropertyImageUrl TestPropertyImageUrl = new PropertyImageUrl();        

        private static readonly PropertyLanguage TestPropertyLanguage = new PropertyLanguage()
        {
            Value = "EN"
        };

        private static readonly PropertyUrl TestPropertyUrl = new PropertyUrl();

        private static readonly PropertyVirtualLink TestPropertyVirtualLink = new PropertyVirtualLink()
        {
            Value = "~/test-url"
        };     

        private static readonly PropertyCheckBoxList TestPropertyCheckBoxList = new PropertyCheckBoxList()
        {
            Value = "Item1,Item2"
        };

        private static readonly PropertyDateList TestPropertyDateList = new PropertyDateList()
        {
            List = new List<DateTime> { DateTime.Now, DateTime.Now.AddDays(1) }
        };

        private static readonly PropertySelector TestPropertySelector = new PropertySelector()
        {
            Value = "Item1,Item2"
        };

        private static readonly PropertyAppSettingsMultiple TestPropertyAppSettingsMultiple = new PropertyAppSettingsMultiple()
        {
            Value = "App1,App2"
        };
    }    
}
