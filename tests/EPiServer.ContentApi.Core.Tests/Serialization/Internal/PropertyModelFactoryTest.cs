using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Serialization.Internal
{
    public class PropertyModelFactoryTest
    {
        [Fact]
        public void Create_WhenPropertyModelHasDefaultConstructor_ShouldCreate()
        {
            Assert.IsType<WithDefaultContructor>(new PropertyModelFactory().Create(typeof(WithDefaultContructor), new PropertyString(), new TestConverterContext()));
        }

        [Fact]
        public void Create_WhenPropertyModelHasConstructorWithPropertyData_ShouldCreateAndPassInPropertyData()
        {
            var property = new PropertyString() { Name = "SomeProperty" };
            var model = new PropertyModelFactory().Create(typeof(WithPropertyDataConstructor), property, new TestConverterContext());
            Assert.IsType<WithPropertyDataConstructor>(model);
            Assert.Same(property.Name, model.Name);
        }

        [Fact]
        public void Create_WhenPropertyModelHasConstructorWithPropertyDataAndBool_ShouldCreateAndPassInPropertyDataAndBoolFromContext()
        {
            var property = new PropertyString() { Name = "SomeProperty" };
            var model = new PropertyModelFactory().Create(typeof(WithPropertyDataAndBoolConstructor), property, new TestConverterContext(excludePersonalization:true));
            Assert.IsType<WithPropertyDataAndBoolConstructor>(model);
            Assert.Equal(property.Name, model.Name);
            Assert.True((model as IPersonalizableProperty).ExcludePersonalizedContent);
        }

        [Fact]
        public void Create_WhenPropertyModelHasConstructorWithPropertyDataAndContext_ShouldCreateAndPassInPropertyDataAndContext()
        {
            var property = new PropertyString() { Name = "SomeProperty" };
            var context = new TestConverterContext(excludePersonalization: true);
            var model = new PropertyModelFactory().Create(typeof(WithPropertyDataAndContextConstructor), property, context);
            Assert.IsType<WithPropertyDataAndContextConstructor>(model);
            Assert.Equal(property.Name, model.Name);
            Assert.Same(context, (model as WithPropertyDataAndContextConstructor).ExposeContext());
        }

        [Fact]
        public void Create_IfPropertyHasUnknownConstructor_ShouldThrow()
        {
            Assert.Throws<InvalidOperationException>(() => new PropertyModelFactory().Create(typeof(WithNotSupportedConstructor), new PropertyString(), new TestConverterContext()));
        }
    }

    public class WithDefaultContructor : IPropertyModel
    {
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string PropertyDataType => throw new NotImplementedException();
    }

    public class WithPropertyDataConstructor : PropertyModel<string, PropertyString>
    {
        public WithPropertyDataConstructor(PropertyString type) : base(type)
        {
        }
    }

    public class WithPropertyDataAndBoolConstructor : PersonalizablePropertyModel<string, PropertyString>
    {
#pragma warning disable CS0618 // Type or member is obsolete
        public WithPropertyDataAndBoolConstructor(PropertyString propertyData, bool excludePersonalizedContent) 
            : base(propertyData, excludePersonalizedContent)
#pragma warning restore CS0618 // Type or member is obsolete
        {
        }
    }

    public class WithPropertyDataAndContextConstructor : PersonalizablePropertyModel<string, PropertyString>
    {
        public WithPropertyDataAndContextConstructor(PropertyString propertyData, ConverterContext converterContext) 
            : base(propertyData, converterContext)
        {
        }

        public ConverterContext ExposeContext() => ConverterContext;
    }

    public class WithNotSupportedConstructor : IPersonalizableProperty
    {
        public WithNotSupportedConstructor(bool excludePersonalizedContent)
        {
            ExcludePersonalizedContent = excludePersonalizedContent;
        }
        public bool ExcludePersonalizedContent { get; set; }
    }
}
