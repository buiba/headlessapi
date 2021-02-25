using System;

namespace EPiServer.ContentManagementApi.Models.Internal
{
    /// <summary>
    /// Contains information of a content reference.
    /// </summary>
    public class ContentReferenceInputModel
    {
        /// <summary>
        /// Id number of the content.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// The version id of the content.
        /// </summary>
        public int? WorkId { get; set; }

        /// <summary>
        /// The unique identifier of the content.
        /// </summary>
        public Guid? GuidValue { get; set; }

        /// <summary>
        /// The provider name that serves the content.
        /// </summary>
        public string ProviderName { get; set; }
    }
}
