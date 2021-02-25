using System.ComponentModel.DataAnnotations;

namespace EPiServer.ContentManagementApi.Models.Internal
{
    /// <summary>
    /// Model for moving content.
    /// </summary>
    public class MoveContentModel
    {
        /// <summary>
        /// Parent link of the content.
        /// </summary>
        [Required]
        public ContentReferenceInputModel ParentLink { get; set; }
    }
}
