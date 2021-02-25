namespace EPiServer.ContentManagementApi.Internal
{
    /// <summary>
    /// List of known problems that are returned by the ContentManagement API.
    /// </summary>
    public static class ProblemCode
    {
        /// <summary>
        /// Indicates that the provided parent content was invalid
        /// </summary>
        public const string InvalidParent = "InvalidParent";

        /// <summary>
        /// Indicates that an operation illegal for system contents was attempted
        /// </summary>
        public const string SystemContent = "SystemContent";

        /// <summary>
        /// Indicates that the validation find any issues with the content.
        /// </summary>
        public const string ContentValidation = "ContentValidation";

        /// <summary>
        /// Indicates that the provided content is read-only
        /// </summary>
        public const string ReadOnlyContent = "ReadOnlyContent";

        /// <summary>
        /// Indicates that the provided content is not allowed to be created under the parent
        /// </summary>
        public const string NotAllowedParent = "NotAllowedParent";

        /// <summary>
        /// Indicates that the status transition was invalid or content does not implement ILocalizable
        /// </summary>
        public const string StatusTransition = "StatusTransition";

        /// <summary>
        /// Indicates that content does not implement IRoutable
        /// </summary>
        public const string ContentNotRoutable = "ContentNotRoutable";

        /// <summary>
        /// Indicates that content does not implement IVersionable
        /// </summary>
        public const string ContentNotVersionable = "ContentNotVersionable";

        /// <summary>
        /// Indicates that content does not implement ILocalizable
        /// </summary>
        public const string ContentNotLocalizable = "ContentNotLocalizable";

        /// <summary>
        /// Indicates that the Content Provider has NOT required capabiliity
        /// </summary>
        public const string ContentProvider = "ContentProvider";

        /// <summary>
        /// Indicates that the action is invalid on a specific content
        /// </summary>
        public const string InvalidAction = "InvalidAction";

        /// <summary>
        /// Indicates that StartPublish must be set when content item is set for scheduled publishing
        /// </summary>
        public const string ScheduledPublishing = "ScheduledPublishing";

        /// <summary>
        /// Indicates that cannot force new version and force current version at the same time.
        /// </summary>
        public const string ForceVersion = "ForceVersion";

        /// <summary>
        /// Indicates that the delayed published flag must be used in combination with the check-in flag.
        /// </summary>
        public const string DelayedPulish = "DelayedPulish";

        /// <summary>
        /// Indicates that the property content reference doesn't exist.
        /// </summary>
        public const string PropertyReferenceNotFound  = "PropertyReferenceNotFound";
    }
}
