using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.DefinitionsApi.Configuration;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

namespace EPiServer.DefinitionsApi.IntegrationTests.TestSetup
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServerTestInitialization))]
    internal class DefinitionsApiTestInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            // Accept anonymous calls during test
            ServiceLocator.Current.GetInstance<DefinitionsApiOptions>()
                .ClearAllowedScopes();
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}
