using EPiServer.Core;
using System;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="IPersonalizableProperty"/>
    /// </summary>
    public abstract partial class PersonalizablePropertyModel<TValue, TType> : PropertyModel<TValue, TType>, IPersonalizableProperty where TType : PropertyData
    {
        internal PersonalizablePropertyModel() { }

        public bool ExcludePersonalizedContent { get; set; }
        
        protected ConverterContext ConverterContext { get; }

        protected PersonalizablePropertyModel(TType propertyData, ConverterContext converterContext) : base(propertyData)
        {
            ExcludePersonalizedContent = converterContext.ExcludePersonalizedContent;
            ConverterContext = converterContext;
        }
    }
}
