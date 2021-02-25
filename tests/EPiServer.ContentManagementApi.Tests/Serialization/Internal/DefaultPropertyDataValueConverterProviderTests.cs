using System.Collections.Generic;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentManagementApi.Serialization.Internal.Converters;
using EPiServer.Core;
using EPiServer.SpecializedProperties;
using Xunit;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    public class DefaultPropertyDataValueConverterProviderTests
    {
        private readonly IEnumerable<IPropertyDataValueConverter> _converters = new List<IPropertyDataValueConverter>
        {
            new SimplePropertyDataValueConverter(),
            new PropertyDataValueConverterTest()
        };

        private DefaultPropertyDataValueConverterProvider Subject()
        {
            var provider = new DefaultPropertyDataValueConverterProvider(_converters);
            provider.RegisterConverters();
            return provider;
        }

        [Fact]
        public void Resolve_IfPropertyModelIsNull_ShouldReturnNull()
        {
            var subject = Subject();
            var converter = subject.Resolve(null);

            Assert.Null(converter);
        }

        [Fact]
        public void Resolve_IfNotHasConverter_ShouldReturnNull()
        {
            var subject = Subject();
            var converter = subject.Resolve(new PropertyModel2());

            Assert.Null(converter);
        }        

        [Fact]
        public void Resolve_IfHasConverter_ShouldReturnConverter()
        {
            var subject = Subject();
            var converter1 = subject.Resolve(new PropertyModel1());

            Assert.NotNull(converter1);
            Assert.IsType<PropertyDataValueConverterTest>(converter1);

            var converter3 = subject.Resolve(new PropertyModel3());

            Assert.NotNull(converter3);
            Assert.IsType<PropertyDataValueConverterTest>(converter3);
        }

        [Theory]
        [MemberData(nameof(SimplePropertyModels))]
        public void Resolve_IfIsSimpleValuePropertyModel_ShouldReturnSimpleValuePropertyDataConverter(IPropertyModel propertyModel)
        {
            var subject = Subject();
            var converter = subject.Resolve(propertyModel);

            Assert.NotNull(converter);
            Assert.IsType<SimplePropertyDataValueConverter>(converter);            
        }

        public static TheoryData SimplePropertyModels => new TheoryData<IPropertyModel>
        {
            new StringPropertyModel(new PropertyString()),
            new LongStringPropertyModel(new PropertyLongString()),
            new AppSettingsPropertyModel(new PropertyAppSettings()),
            new BlobPropertyModel(new PropertyBlob()),
            new DocumentUrlPropertyModel(new PropertyDocumentUrl()),
            new DropDownListPropertyModel(new PropertyDropDownList()),
            new ImageUrlPropertyModel(new PropertyImageUrl()),
            new LanguagePropertyModel(new PropertyLanguage()),
            new UrlPropertyModel(new PropertyUrl()),
            new VirtualLinkPropertyModel(new PropertyVirtualLink()),
            new NumberPropertyModel(new PropertyNumber(0)),
            new FileSortOrderPropertyModel(new PropertyFileSortOrder()),
            new FloatPropertyModel(new PropertyFloatNumber()),
            new FramePropertyModel(new PropertyFrame()),
            new SortOrderPropertyModel(new PropertySortOrder()),
            new WeekdayPropertyModel(new PropertyWeekDay()),
            new BooleanPropertyModel(new PropertyBoolean()),
            new CheckboxListPropertyModel(new PropertyCheckBoxList()),
            new DateListPropertyModel(new PropertyDateList()),
            new DateTimePropertyModel(new PropertyDate()),
            new DoubleListPropertyModel(new PropertyDoubleList()),
            new IntegerListPropertyModel(new PropertyIntegerList()),           
            new SelectorPropertyModel(new PropertySelector()),
            new StringListPropertyModel(new PropertyStringList()),
            new AppSettingsMultiplePropertyModel(new PropertyAppSettingsMultiple())            
        };
    }
}
