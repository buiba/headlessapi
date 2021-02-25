#pragma warning disable SA1611 // Element parameters should be documented
#pragma warning disable SA1604 // Element documentation should have summary

using System;
using System.Runtime.Serialization;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    /// <exclude />
    [Serializable]
    public sealed class MultipleDefinitionMatchedException : Exception
    {
        /// <exclude />
        public MultipleDefinitionMatchedException() : base()
        {
        }

        /// <exclude />
        public MultipleDefinitionMatchedException(string message) : base(message)
        {
        }

        /// <exclude />
        public MultipleDefinitionMatchedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        private MultipleDefinitionMatchedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
