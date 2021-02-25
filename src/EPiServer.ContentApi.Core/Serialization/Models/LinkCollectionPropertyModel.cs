using System.Collections.Generic;
using System.Linq;
using EPiServer.Security;
using EPiServer.SpecializedProperties;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Security;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Mapped property model for <see cref="PropertyLinkCollection"/>
    /// </summary>
    public partial class LinkCollectionPropertyModel : CollectionPropertyModelBase<LinkItemNode, PropertyLinkCollection> 
    {
        [JsonConstructor]
        internal LinkCollectionPropertyModel() { }

        public LinkCollectionPropertyModel
        (
            PropertyLinkCollection propertyLinkCollection, 
           ConverterContext converterContext)  : base(propertyLinkCollection, converterContext)
        {

        }

        public LinkCollectionPropertyModel(PropertyLinkCollection propertyLinkCollection, ConverterContext converterContext,
			ContentLoaderService contentLoaderService,
            ContentConvertingService contentConvertingService,
            IContentAccessEvaluator accessEvaluator,
			ISecurityPrincipal principalAccessor) : base(propertyLinkCollection, converterContext, contentLoaderService, contentConvertingService, accessEvaluator, principalAccessor)
        {

        }

        /// <inheritdoc />
        protected override IEnumerable<LinkItemNode> GetValue()
        {
            return  (_propertyLongString.Links == null || !_propertyLongString.Links.Any()) ?
            new List<LinkItemNode>() :
            _propertyLongString.Links.Select(x => new LinkItemNode(x.Href, x.Title, x.Target, x.Text));
        }
    }
}
