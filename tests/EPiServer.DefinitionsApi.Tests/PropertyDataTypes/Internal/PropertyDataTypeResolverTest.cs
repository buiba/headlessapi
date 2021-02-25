using System;
using System.Linq;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.SpecializedProperties;
using Moq;
using Xunit;

namespace EPiServer.DefinitionsApi.PropertyDataTypes.Internal
{
    public class PropertyDataTypeResolverTest
    {
        public static TheoryData PropertyDefinitionMappings => new TheoryData<PropertyDefinitionType, ExternalPropertyDataType>
        {
            {
                new PropertyDefinitionType { ID = 1, Name = "Number", TypeName = typeof(PropertyNumber).FullName, AssemblyName = typeof(PropertyNumber).Assembly.GetName().Name },
                new ExternalPropertyDataType(nameof(PropertyNumber))
            },
            {
                new PropertyDefinitionType { ID = 1, Name = "FloatNumber", TypeName = typeof(PropertyFloatNumber).FullName, AssemblyName = typeof(PropertyFloatNumber).Assembly.GetName().Name },
                new ExternalPropertyDataType(nameof(PropertyFloatNumber))
            },
            {
                new PropertyDefinitionType { ID = 1, Name = "Date", TypeName = typeof(PropertyDate).FullName, AssemblyName = typeof(PropertyDate).Assembly.GetName().Name },
                new ExternalPropertyDataType(nameof(PropertyDate))
            },
            {
                new PropertyDefinitionType { ID = 1, Name = "LongString", TypeName = typeof(PropertyLongString).FullName, AssemblyName = typeof(PropertyLongString).Assembly.GetName().Name },
                new ExternalPropertyDataType(nameof(PropertyLongString))
            },
            {
                new PropertyDefinitionType { ID = 1, Name = "XhtmlString", TypeName = typeof(PropertyXhtmlString).FullName, AssemblyName = typeof(PropertyXhtmlString).Assembly.GetName().Name },
                new ExternalPropertyDataType(nameof(PropertyXhtmlString))
            },
            {
                new PropertyDefinitionType { ID = 1, Name = "ContentArea", TypeName = typeof(PropertyContentArea).FullName, AssemblyName = typeof(PropertyContentArea).Assembly.GetName().Name },
                new ExternalPropertyDataType(nameof(PropertyContentArea))
            },
            {
                new PropertyDefinitionType { ID = 1, Name = "SomeBlock", DataType = PropertyDataType.Block, TypeName = typeof(PropertyBlock<SomeBlock>).FullName, AssemblyName = typeof(PropertyBlock<SomeBlock>).Assembly.GetName().Name },
                ExternalPropertyDataType.Block(nameof(SomeBlock))
            },
            {
                new PropertyDefinitionType { ID = 1, Name = "LinkCollection", DataType = PropertyDataType.LinkCollection, TypeName = typeof(PropertyLinkCollection).FullName, AssemblyName = typeof(PropertyLinkCollection).Assembly.GetName().Name },
                new ExternalPropertyDataType(nameof(PropertyLinkCollection))
            },
            {
                new PropertyDefinitionType { ID = 1, Name = "Category", DataType = PropertyDataType.Category, TypeName = typeof(PropertyCategory).FullName, AssemblyName = typeof(PropertyCategory).Assembly.GetName().Name },
                new ExternalPropertyDataType(nameof(PropertyCategory))
            },
            {
                new PropertyDefinitionType { ID = 1, Name = "ContentReferenceList", TypeName = typeof(PropertyContentReferenceList).FullName, AssemblyName = typeof(PropertyContentReferenceList).Assembly.GetName().Name },
                new ExternalPropertyDataType(nameof(PropertyContentReferenceList))
            },
            {
                new PropertyDefinitionType { ID = 1, Name = "SomeList", TypeName = typeof(SomeListType).FullName, AssemblyName = typeof(SomeListType).Assembly.GetName().Name },
                new ExternalPropertyDataType(nameof(SomeListType))
            },
            {
                new PropertyDefinitionType { ID = 1, Name = "Url", TypeName = typeof(PropertyUrl).FullName, AssemblyName = typeof(PropertyUrl).Assembly.GetName().Name },
                new ExternalPropertyDataType(nameof(PropertyUrl))
            }
        };

        [Theory]
        [MemberData(nameof(PropertyDefinitionMappings))]
        public void ToExternalPropertyDataType_WithDefinitionType_ShouldReturnExternalDataType(PropertyDefinitionType source, ExternalPropertyDataType expected)
        {
            Assert.Equal(expected, Subject().ToExternalPropertyDataType(source));
        }

        [Fact]
        public void Resolve_WhenDataTypeNameMatchesExistingPropertyDefinitionType_ShouldReturnPropertyDefinitionType()
        {
            var result = Subject().Resolve(new ExternalPropertyDataType(nameof(PropertyNumber)));
            Assert.Equal(typeof(PropertyNumber), result.DefinitionType);
        }

        [Fact]
        public void Resolve_WhenExternalPropertyIsBlockWithItem_ShouldReturnPropertyBlockDefinitionType()
        {
            var result = Subject().Resolve(ExternalPropertyDataType.Block(nameof(SomeBlock)));
            Assert.Equal(typeof(PropertyBlock<SomeBlock>), result.DefinitionType);
        }

        [Fact]
        public void List_ShouldReturnAllTypes()
        {
            var propertyDefintionTypes = new[]
            {
                new PropertyDefinitionType { ID = 1, Name = "Number", TypeName = typeof(PropertyNumber).FullName, AssemblyName = typeof(PropertyNumber).Assembly.GetName().Name },
                new PropertyDefinitionType { ID = 2, Name = "Date", TypeName = typeof(PropertyDate).FullName, AssemblyName = typeof(PropertyDate).Assembly.GetName().Name },
                new PropertyDefinitionType { ID = 3, Name = "SomeBlock", DataType = PropertyDataType.Block, TypeName = typeof(PropertyBlock<SomeBlock>).FullName, AssemblyName = typeof(PropertyBlock<SomeBlock>).Assembly.GetName().Name },
            };

            var propertyDefinitionTypeRepository = new Mock<IPropertyDefinitionTypeRepository>();
            propertyDefinitionTypeRepository.Setup(p => p.List()).Returns(propertyDefintionTypes);

            var subject = new PropertyDataTypeResolver(propertyDefinitionTypeRepository.Object);
            var result = subject.List();

            var expected = new[]
            {
                new ExternalPropertyDataType(nameof(PropertyNumber)),
                new ExternalPropertyDataType(nameof(PropertyDate)),
                ExternalPropertyDataType.Block(nameof(SomeBlock)),
            };
            Assert.Equal(expected, result.ToArray());
        }

        private PropertyDataTypeResolver Subject()
        {
            var propertyDefinitionTypes = new[]
            {
                new PropertyDefinitionType { ID = 1, Name = "Number", TypeName = typeof(PropertyNumber).FullName, AssemblyName = typeof(PropertyNumber).Assembly.GetName().Name },
                new PropertyDefinitionType { ID = 2, Name = "FloatNumber", TypeName = typeof(PropertyFloatNumber).FullName, AssemblyName = typeof(PropertyFloatNumber).Assembly.GetName().Name },
                new PropertyDefinitionType { ID = 3, Name = "Date", TypeName = typeof(PropertyDate).FullName, AssemblyName = typeof(PropertyDate).Assembly.GetName().Name },
                new PropertyDefinitionType { ID = 4, Name = "SomeBlock", DataType = PropertyDataType.Block, TypeName = typeof(PropertyBlock<SomeBlock>).FullName, AssemblyName = typeof(PropertyBlock<SomeBlock>).Assembly.GetName().Name },
                new PropertyDefinitionType { ID = 5, Name = "LinkCollection", DataType = PropertyDataType.LinkCollection, TypeName = typeof(PropertyLinkCollection).FullName, AssemblyName = typeof(PropertyLinkCollection).Assembly.GetName().Name },
                new PropertyDefinitionType { ID = 6, Name = "DateList", DataType = PropertyDataType.Json, TypeName = typeof(PropertyDateList).FullName, AssemblyName = typeof(PropertyDateList).Assembly.GetName().Name },
                new PropertyDefinitionType { ID = 7, Name = "String", DataType = PropertyDataType.String, TypeName = typeof(PropertyString).FullName, AssemblyName = typeof(PropertyString).Assembly.GetName().Name },
                new PropertyDefinitionType { ID = 8, Name = "LongString", DataType = PropertyDataType.LongString, TypeName = typeof(PropertyLongString).FullName, AssemblyName = typeof(PropertyLongString).Assembly.GetName().Name },
                new PropertyDefinitionType { ID = 9, Name = "XhtmlString", DataType = PropertyDataType.LongString, TypeName = typeof(PropertyXhtmlString).FullName, AssemblyName = typeof(PropertyXhtmlString).Assembly.GetName().Name },
                new PropertyDefinitionType { ID = 1000, Name = "ContentReferenceList", DataType = PropertyDataType.Json, TypeName = typeof(PropertyContentReferenceList).FullName, AssemblyName = typeof(PropertyContentReferenceList).Assembly.GetName().Name },
                new PropertyDefinitionType { ID = 1001, Name = "SomeList", DataType = PropertyDataType.Json, TypeName = typeof(SomeListType).FullName, AssemblyName = typeof(SomeListType).Assembly.GetName().Name },
                new PropertyDefinitionType { ID = 1002, Name = "SomeOtherList", DataType = PropertyDataType.Json, TypeName = typeof(SomeOtherNamespace.SomeOtherListType).FullName, AssemblyName = typeof(SomeOtherNamespace.SomeOtherListType).Assembly.GetName().Name },
                new PropertyDefinitionType { ID = 1003, Name = "OtherList", DataType = PropertyDataType.Json, TypeName = typeof(OtherListType).FullName, AssemblyName = typeof(OtherType).Assembly.GetName().Name },
                new PropertyDefinitionType { ID = 1004, Name = "Category", DataType = PropertyDataType.Category, TypeName = typeof(PropertyCategory).FullName, AssemblyName = typeof(PropertyCategory).Assembly.GetName().Name },
                new PropertyDefinitionType { ID = 1005, Name = "ContentArea", TypeName = typeof(PropertyContentArea).FullName, AssemblyName = typeof(PropertyContentArea).Assembly.GetName().Name },
                new PropertyDefinitionType { ID = 1006, Name = "Url", TypeName = typeof(PropertyUrl).FullName, AssemblyName = typeof(PropertyUrl).Assembly.GetName().Name },
            };

            var propertyDefinitionTypeRepository = new Mock<IPropertyDefinitionTypeRepository>();
            propertyDefinitionTypeRepository.Setup(p => p.List()).Returns(propertyDefinitionTypes);
            propertyDefinitionTypeRepository.Setup(l => l.Load(It.IsAny<Type>())).Returns<Type>(t => propertyDefinitionTypes.FirstOrDefault(x => x.DefinitionType == t));

            return new PropertyDataTypeResolver(propertyDefinitionTypeRepository.Object);
        }
    }

    public class SomeBlock : BlockData { }
    public class SomeType { }
    public class SomeListType : PropertyList<SomeType> { }

    public class OtherType { }
    public class OtherListType : PropertyList<OtherType> { }
}
namespace SomeOtherNamespace
{
    public class SomeType { }
    public class SomeOtherListType : PropertyList<SomeType> { }
}
