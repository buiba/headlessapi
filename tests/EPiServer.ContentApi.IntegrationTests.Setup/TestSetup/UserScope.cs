using System;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using Xunit.Sdk;

namespace EPiServer.ContentApi.IntegrationTests.TestSetup
{
    public class UserScope : IDisposable
    {
        private readonly IPrincipal _currentUser;

        public UserScope(string user, params string[] roles)
        {
            _currentUser = Thread.CurrentPrincipal;
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(user), roles ?? new string[0]);
        }
        public void Dispose() => Thread.CurrentPrincipal = _currentUser;
    }
    public class UserScopeAttribute : BeforeAfterTestAttribute
    {
        private readonly string _user;
        private readonly string[] _roles;
        private IPrincipal _currentUser;

        public UserScopeAttribute(string user, params string[] roles)
        {
            _user = user;
            _roles = roles;
        }

        public override void Before(MethodInfo methodUnderTest)
        {
            _currentUser = Thread.CurrentPrincipal;
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(_user), _roles ?? Array.Empty<string>());
        }

        public override void After(MethodInfo methodUnderTest)
        {
            Thread.CurrentPrincipal = _currentUser;
        }
    }
}
