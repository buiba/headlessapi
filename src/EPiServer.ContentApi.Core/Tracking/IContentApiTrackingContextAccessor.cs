namespace EPiServer.ContentApi.Core.Tracking
{
    /// <summary>
    /// Accessor to get the <see cref="ContentApiTrackingContext"/> that is associated with current request
    /// </summary>
    public interface IContentApiTrackingContextAccessor
    {
        /// <summary>
        /// The <see cref="ContentApiTrackingContext"/> associated with current request
        /// </summary>
        ContentApiTrackingContext Current { get; }
    }
}
