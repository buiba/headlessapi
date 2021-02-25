using System;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// Serializable model class transfer content type information.
    /// </summary>
    [Obsolete("This preview class will be removed in an upcoming version")]
    public class ContentTypeModel
    {
        /// <summary>
        /// Gets or sets the name of the content type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the external identifier of the content type.
        /// </summary>
        public Guid? GuidValue { get; set; }

        /// <summary>
        /// Gets or sets the content type base.
        /// </summary>
        public string Base { get; set; }
    }
}
