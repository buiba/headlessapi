using System;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.Core;
using EPiServer.SpecializedProperties;

namespace EPiServer.ContentManagementApi.Serialization.Internal
{
    internal class PropertyModel1 : IPropertyModel
    {
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string PropertyDataType => throw new NotImplementedException();
    }

    internal class PropertyModel2 : IPropertyModel
    {
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string PropertyDataType => throw new NotImplementedException();
    }

    internal class PropertyModel3 : IPropertyModel
    {
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string PropertyDataType => throw new NotImplementedException();
    }

    internal class PropertyModel4 : IPropertyModel
    {
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string PropertyDataType => throw new NotImplementedException();
    }

    internal class PropertyModel5 : IPropertyModel
    {
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string PropertyDataType => throw new NotImplementedException();
    }

    public class NotPropertyBlock : PropertyData
    {
        public override object Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override PropertyDataType Type => throw new NotImplementedException();

        public override Type PropertyValueType => throw new NotImplementedException();

        public override void ParseToSelf(string value)
        {
            throw new NotImplementedException();
        }

        protected override void SetDefaultValue()
        {
        }
    }

    public class OuterBlock : BlockData
    {
        public string Heading { get; set; }
        public InnerBlock SomeNestedBlock { get; set; }
    }

    public class InnerBlock: BlockData
    {
        public string Title { get; set; }
    }

    public class CustomBlockProperty : PropertyBlock<BlockData>
    {
        public CustomBlockProperty(BlockData blockData) : base(blockData)
        {

        }
    }

    [PropertyDataValueConverter(new Type[] { typeof(PropertyModel1), typeof(PropertyModel3), typeof(PropertyModel4) })]
    internal class PropertyDataValueConverterTest : IPropertyDataValueConverter
    {
        public object Convert(IPropertyModel propertyModel, PropertyData propertyData)
        {
            throw new NotImplementedException();
        }
    }    
}
