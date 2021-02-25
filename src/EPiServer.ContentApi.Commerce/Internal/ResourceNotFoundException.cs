using System;

namespace EPiServer.ContentApi.Commerce.Internal
{
    [Serializable]
    internal class ResourceNotFoundException : Exception
    {

        public ResourceNotFoundException()
            : base("Resource was not found")
        {}

        public ResourceNotFoundException(string resourceId)
            : base($"Resource {resourceId} was not found")
        { }


        public ResourceNotFoundException(Exception innerException)
            : base("Resource was not found", innerException)
        {}
    }
}
