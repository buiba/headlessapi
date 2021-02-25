using System;
using System.Collections.Generic;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.ContentApi.Core.Serialization.Models;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Content
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyGuidList"/>
    /// </summary>
    internal class PropertyGuidListModel : PropertyModel<IList<Guid>, PropertyGuidList>
    {
        public PropertyGuidListModel(PropertyGuidList propertyGuidList) : base(propertyGuidList)
        {
            Value = propertyGuidList.List;
        }
    }
}
