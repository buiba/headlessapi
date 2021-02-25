using System;
using EPiServer.DefinitionsApi.ContentTypes.Internal;
using EPiServer.DefinitionsApi.Manifest;
using EPiServer.ServiceLocation;

namespace EPiServer.DefinitionsApi.ContentTypes.Manifest
{
    [ServiceConfiguration(typeof(IManifestSectionImporter), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class ContentTypesSectionImporter : IManifestSectionImporter
    {
        private readonly ExternalContentTypeRepository _repository;

        public ContentTypesSectionImporter(ExternalContentTypeRepository repository)
        {
            _repository = repository;
        }

        public Type SectionType => typeof(ContentTypesSection);

        public string SectionName => "ContentTypes";

        public int Order => 20;

        public void Import(IManifestSection section, ImportContext importContext)
        {
            if (section is ContentTypesSection contentTypesSection)
            {
                foreach (var contentType in contentTypesSection.Items)
                {
                    if (contentType.Id == Guid.Empty)
                    {
                        var existing = _repository.Get(contentType.Name);

                        // If a content type exist by name then we assumed it is an update.
                        if (existing is object)
                        {
                            contentType.Id = existing.Id;
                        }
                        else
                        {
                            contentType.Id = Guid.NewGuid();
                        }
                    }
                }

                _repository.Save(contentTypesSection.Items, out _, contentTypesSection.AllowedDowngrades, contentTypesSection.AllowedUpgrades);

                if (contentTypesSection.Items.Count == 1)
                {
                    importContext.AddLogMessage($"Imported {contentTypesSection.Items.Count} content type.", ImportLogMessageSeverity.Success);
                }
                else
                {
                    importContext.AddLogMessage($"Imported {contentTypesSection.Items.Count} content types.", ImportLogMessageSeverity.Success);
                }
            }
        }
    }
}
