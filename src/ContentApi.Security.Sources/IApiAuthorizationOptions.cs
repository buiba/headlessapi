using System.Collections.Generic;

namespace EPiServer.ContentApi.Security.Internal
{
    internal interface IApiAuthorizationOptions
    {
        ICollection<string> AllowedScopes { get; }

        string ScopeClaimType { get; }

        string Name { get; }
    }
}
