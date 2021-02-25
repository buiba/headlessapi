using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

namespace EPiServer.DefinitionsApi.Manifest.Internal
{
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    internal class ManifestService
    {
        private static readonly ILogger Log = LogManager.GetLogger(typeof(ManifestService));

        private readonly ManifestSectionImporterResolver _sectionImporterResolver;

        public ManifestService(ManifestSectionImporterResolver sectionImporterResolver)
        {
            _sectionImporterResolver = sectionImporterResolver;
        }

        public void ImportManifestSections(IEnumerable<IManifestSection> sections, ImportContext importContext)
        {
            var importers = new List<KeyValuePair<IManifestSectionImporter, IManifestSection>>();

            foreach (var section in sections)
            {
                var importer = _sectionImporterResolver.Resolve(section);

                if (importer is null)
                {
                    var error = $"There is no manifest section importer (IManifestSectionImporter) registered for the type '{section.GetType()}'.";

                    if (importContext.ContinueOnError)
                    {
                        importContext.AddLogMessage(error, ImportLogMessageSeverity.Error);
                        Log.Error(error);
                        continue;
                    }
                    else
                    {
                        throw new ArgumentException(error);
                    }
                }

                importers.Add(new KeyValuePair<IManifestSectionImporter, IManifestSection>(importer, section));
            }

            foreach (var kvp in importers.OrderBy(x => x.Key.Order))
            {
                try
                {
                    kvp.Key.Import(kvp.Value, importContext);
                }
                catch (Exception ex)
                {
                    if (importContext.ContinueOnError)
                    {
                        var error = $"Import of the manifest section '{kvp.Value.GetType()}' failed.";
                        importContext.AddLogMessage($"{error} {ex.Message}", ImportLogMessageSeverity.Error);
                        Log.Error(error, ex);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
