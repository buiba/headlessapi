using EPiServer.Core;
using System;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="IPersonalizableProperty"/>
    /// </summary>
    public abstract partial class PersonalizablePropertyModel<TValue, TType> : PropertyModel<TValue, TType>, IPersonalizableProperty where TType : PropertyData
    {
        protected PersonalizablePropertyModel(TType propertyData, bool excludePersonalizedContent) : base(propertyData)
        {
            ExcludePersonalizedContent = excludePersonalizedContent;
        }        
    }
}
