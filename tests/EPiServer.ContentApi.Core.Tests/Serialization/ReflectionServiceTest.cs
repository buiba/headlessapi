using EPiServer.ContentApi.Core.Serialization;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAbstraction.RuntimeModel;
using Moq;
using System;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization
{
    public class ReflectionServiceTest
    {
        public Mock<ContentTypeModelRepository> _contentTypeModelRepository;
        public ReflectionService _reflectionService;

        public ReflectionServiceTest()
        {
            _contentTypeModelRepository = new Mock<ContentTypeModelRepository>();
            _reflectionService = new ReflectionService(_contentTypeModelRepository.Object);
        }

        public class CreateInstance : ReflectionServiceTest
        {
            public class And_when_type_parameter_is_null : CreateInstance
            {
                [Fact]
                public void It_should_return_null()
                {
                    var activatedObj = _reflectionService.CreateInstance(null);
                    Assert.Null(activatedObj);
                }
            }

            public class And_when_type_parameter_is_not_null : CreateInstance
            {
                [Fact]
                public void It_should_return_activated_object()
                {
                    var activatedObj = _reflectionService.CreateInstance(typeof(ActivatorTesting));

                    Assert.NotNull(activatedObj);
                    Assert.Equal(activatedObj.GetType().FullName, typeof(ActivatorTesting).FullName);
                }
            }

            public class ActivatorTesting
            {
                public ActivatorTesting()
                {

                }
            }
        }

        public class GetAttributes : ReflectionServiceTest
        {
            public ContentType _contentType;
            public TestPropData _propData;
            public PropertyDefinitionModel _propertyDefinitionModel;

            public GetAttributes()
            {
                _propData = new TestPropData()
                {
                    Name = "test"
                };

                _contentType = new ContentType();
                _contentType.ID = 1;
                _contentType.PropertyDefinitions.Add(new PropertyDefinition()
                {
                    Name = "test"
                });
               
                _propertyDefinitionModel = new PropertyDefinitionModel();
                _propertyDefinitionModel.Attributes.AddAttribute(new TestAttribute()); 

                _contentTypeModelRepository.Setup(repo => repo.GetPropertyModel(It.IsAny<int>(), It.IsAny<PropertyDefinition>()))
                                           .Returns(_propertyDefinitionModel);
            }

            [Fact]
            public void It_should_return_all_attributes()
            {
                var attrList = _reflectionService.GetAttributes(_contentType, _propData);
                Assert.NotNull(attrList);
            }
        }
    }

    public class TestPropData : PropertyData
    {
        public override object Value { get; set; }

        public override PropertyDataType Type { get;  }

        public override Type PropertyValueType { get; }

        public override void ParseToSelf(string value)
        {

        }

        protected override void SetDefaultValue()
        {

        }
    }

    public class TestAttribute : Attribute
    {

    }
}
