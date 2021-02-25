using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ServiceLocation;

namespace EPiServer.DefinitionsApi.Manifest.Internal
{
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    internal class ManifestSectionImporterResolver
    {
        private readonly List<IManifestSectionImporter> _sectionImporters;

        internal ManifestSectionImporterResolver()
            : this(Enumerable.Empty<IManifestSectionImporter>()) { }

        public ManifestSectionImporterResolver(IEnumerable<IManifestSectionImporter> sectionImporters)
        {
            _sectionImporters = sectionImporters.ToList();
        }

        public virtual IManifestSectionImporter Resolve(IManifestSection section)
        {
            return _sectionImporters.Find(i =>
                i.SectionType.Equals(section.GetType()));
        }

        public virtual Type ResolveManifestSectionType(string sectionName)
        {
            return _sectionImporters.Find(i =>
                i.SectionName.Equals(sectionName, StringComparison.OrdinalIgnoreCase))
                ?.SectionType;
        }
    }
}
