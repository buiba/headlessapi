using System;
using EPiServer.ContentManagementApi.Configuration;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentManagementApi.IntegrationTests.TestSetup
{
    public class OptionsScope : IDisposable
    {
        private readonly ContentManagementApiOptions _currentOptions;
        private readonly Func<ContentManagementApiOptions> _optionsAccessor;

        public OptionsScope(Action<ContentManagementApiOptions> configure)
        {
            var apiConfiguration = ServiceLocator.Current.GetInstance<ContentManagementApiOptions>();
            _optionsAccessor = () => apiConfiguration;
            _currentOptions = _optionsAccessor().Clone() as ContentManagementApiOptions;
            configure?.Invoke(_optionsAccessor());
        }

        public void Dispose()
        {
            var defaultOptions = _optionsAccessor();
            defaultOptions.SetRequiredRole(_currentOptions.RequiredRole);
            defaultOptions.ScopeClaimType = _currentOptions.ScopeClaimType;

            defaultOptions.ClearAllowedScopes();
            foreach (var scope in _currentOptions.AllowedScopes)
            {
                defaultOptions.AddAllowedScope(scope);
            }
        }
    }
}
