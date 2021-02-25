namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    ///     Serializable model class for ContentAreaItem 
    /// </summary>
    public class ContentAreaItemModel: IContentItem
    {
        /// <summary>
        /// Display option
        /// </summary>
        public string DisplayOption { get; set; }

        /// <summary>
        /// Tag
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Mapped property for ContentReference
        /// </summary>
        public ContentModelReference ContentLink { get; set; }
    }
}
