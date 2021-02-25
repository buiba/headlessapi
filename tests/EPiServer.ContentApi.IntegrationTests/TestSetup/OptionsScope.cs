using System;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.IntegrationTests.TestSetup
{
    public class OptionsScope : IDisposable
    {
        private readonly ContentApiOptions _currentOptions;
        private readonly Func<ContentApiOptions> _optionsAccessor;

        public OptionsScope(Action<ContentApiOptions> configure)
        {
            var apiConfiguration = ServiceLocator.Current.GetInstance<ContentApiConfiguration>();
            _optionsAccessor = () => apiConfiguration.Default();
            _currentOptions =  _optionsAccessor().Clone() as ContentApiOptions;

            configure?.Invoke(_optionsAccessor());
        }

        public OptionsScope(bool useOptimizedOptions)
        {
            var apiConfiguration = ServiceLocator.Current.GetInstance<ContentApiConfiguration>();
            _optionsAccessor = () => apiConfiguration.Default();
            _currentOptions = _optionsAccessor().Clone() as ContentApiOptions;

            if (!useOptimizedOptions)
            {
                _optionsAccessor()
                    .SetValidateTemplateForContentUrl(true)
                    .SetFlattenPropertyModel(false)
                    .SetEnablePreviewMode(false)
                    .SetIncludeNullValues(true)
                    .SetIncludeMasterLanguage(true)
                    .SetIncludeNumericContentIdentifier(true);
            }
            else if (useOptimizedOptions)
            {
                _optionsAccessor()
                    .SetValidateTemplateForContentUrl(false)
                    .SetFlattenPropertyModel(true)
                    .SetEnablePreviewMode(true)
                    .SetIncludeNullValues(false)
                    .SetIncludeMasterLanguage(false)
                    .SetIncludeNumericContentIdentifier(false);
            }
        }

        public ContentApiOptions GetScopedApiOptions()
        {
            return _optionsAccessor();
        }

        public void Dispose()
        {
            var defaultOptions = _optionsAccessor();
            defaultOptions.SetClients(_currentOptions.Clients);
            defaultOptions.SetFlattenPropertyModel(_currentOptions.FlattenPropertyModel);
            defaultOptions.SetMinimumRoles(_currentOptions.MinimumRoles);
            defaultOptions.SetMultiSiteFilteringEnabled(_currentOptions.MultiSiteFilteringEnabled);
            defaultOptions.SetRequiredRole(_currentOptions.RequiredRole);
            defaultOptions.SetSiteDefinitionApiEnabled(_currentOptions.SiteDefinitionApiEnabled);
            defaultOptions.SetValidateTemplateForContentUrl(_currentOptions.ValidateTemplateForContentUrl);
            defaultOptions.SetEnablePreviewMode(_currentOptions.EnablePreviewMode);
            defaultOptions.SetIncludeNullValues(_currentOptions.IncludeNullValues);
            defaultOptions.SetIncludeMasterLanguage(_currentOptions.IncludeMasterLanguage);
        }
    }
    
}
