using System;
using System.Collections.Generic;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;

namespace EPiServer.ContentApi.Core.Serialization
{
    //Plan is to obsolete this as [Obsolete("Has been replaced by IContentConverter and IPropertyConverter")]

    /// <summary>
    /// Interface for mapping content to custom models
    /// </summary>
    public interface IContentModelMapper
    {
		/// <summary>
		/// List property model converters to convert EpiServer's <see cref="PropertyData"/> to Content Api's property model
		/// </summary>
        IEnumerable<IPropertyModelConverter> PropertyModelConverters { get; }

        /// <summary>
        /// Maps an instance of IContent to ContentApiModel
        /// </summary>
        /// <param name="content">The IContent object that the ContentModel is generated from</param>
        /// <param name="excludePersonalizedContent">Boolean to indicate whether or not to return personalization data in the instance of the ContentApiModel</param>
        /// <param name="expand"> String contains properties need to be expanded, seperated by comma. Eg: expand=MainContentArea,productPageLinks. Pass expand='*' to expand all properties</param>
        /// <returns>Instance of ContentModel</returns>
        ContentApiModel TransformContent(IContent content, bool excludePersonalizedContent = false, string expand = "");
    }
}
