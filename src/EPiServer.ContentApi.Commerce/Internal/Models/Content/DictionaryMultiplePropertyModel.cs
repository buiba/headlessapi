using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.ContentApi.Core.Serialization.Models;

namespace EPiServer.ContentApi.Commerce.Internal.Models.Content
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyDictionaryMultiple"/>
    /// </summary>
    internal class DictionaryMultiplePropertyModel : PropertyModel<IEnumerable<string>, PropertyDictionaryMultiple>
    {
        public DictionaryMultiplePropertyModel(PropertyDictionaryMultiple propertyDictionaryMultiple) : base(propertyDictionaryMultiple)
        {
            Value = propertyDictionaryMultiple.Items.Select(x => x.ToString());
        }
    }
}
