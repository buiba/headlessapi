using System;
using EPiServer.DefinitionsApi.Manifest;
using EPiServer.DefinitionsApi.PropertyGroups.Internal;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

namespace EPiServer.DefinitionsApi.PropertyGroups.Manifest
{
    [ServiceConfiguration(typeof(IManifestSectionImporter), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class PropertyGroupsSectionImporter : IManifestSectionImporter
    {
        private static readonly ILogger Log = LogManager.GetLogger(typeof(PropertyGroupsSectionImporter));

        private readonly PropertyGroupRepository _propertyGroupRepository;

        public PropertyGroupsSectionImporter(PropertyGroupRepository propertyGroupRepository)
        {
            _propertyGroupRepository = propertyGroupRepository;
        }

        public Type SectionType => typeof(PropertyGroupsSection);

        public string SectionName => "PropertyGroups";

        public int Order => 10;

        public void Import(IManifestSection section, ImportContext importContext)
        {
            if (section is PropertyGroupsSection propertyGroupsDefinitions)
            {
                var counter = 0;
                var hasLoggedMessage = false;

                foreach (var propertyGroup in propertyGroupsDefinitions.Items)
                {
                    try
                    {
                        _propertyGroupRepository.Save(propertyGroup);
                        counter++;
                    }
                    catch (Exception ex)
                    {
                        if (importContext.ContinueOnError)
                        {
                            var log = $"Import of the property group '{propertyGroup.Name}' failed.";
                            importContext.AddLogMessage($"{log} {ex.Message}", ImportLogMessageSeverity.Error);
                            Log.Error(log, ex);

                            hasLoggedMessage = true;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                if (counter == 0 && hasLoggedMessage == false)
                {
                    importContext.AddLogMessage("Imported 0 property groups.", ImportLogMessageSeverity.Success);
                }
                else if (counter == 1)
                {
                    importContext.AddLogMessage($"Imported {counter} property group.", ImportLogMessageSeverity.Success);
                }
                else if (counter > 1)
                {
                    importContext.AddLogMessage($"Imported {counter} property groups.", ImportLogMessageSeverity.Success);
                }
            }
        }
    }
}
