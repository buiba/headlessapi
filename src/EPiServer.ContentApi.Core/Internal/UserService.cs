using EPiServer.Core;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using System.Security.Principal;

namespace EPiServer.ContentApi.Core.Internal
{
    /// <summary>
    /// Handle user-related logic
    /// </summary>
    [ServiceConfiguration(typeof(UserService))]
    public class UserService
    {
        protected readonly IContentAccessEvaluator _accessEvaluator;

        public UserService(IContentAccessEvaluator accessEvaluator)
        {
            _accessEvaluator = accessEvaluator;
        }

        /// <summary>
        /// Check whether user has the given privilege to access a content
        /// </summary>
        public virtual bool IsUserAllowedToAccessContent(IContent content, IPrincipal principal, AccessLevel accessLevel)
            => _accessEvaluator.HasAccess(content, principal, accessLevel);
    }
}
