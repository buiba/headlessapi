using System;

namespace EPiServer.DefinitionsApi.Manifest
{
    /// <summary>
    /// Defines a manifest section importer.
    /// </summary>
    public interface IManifestSectionImporter
    {
        /// <summary>
        /// Gets the manifest section type this importer handles.
        /// </summary>
        Type SectionType { get; }

        /// <summary>
        /// Gets the manifest section name (JSON property) this importer handles.
        /// </summary>
        string SectionName { get; }

        /// <summary>
        /// Gets the order in which this importer should run.
        /// </summary>
        int Order { get; }

        /// <summary>
        /// Imports the provided manifest section.
        /// </summary>
        /// <param name="section">The manifest section to import.</param>
        /// <param name="importContext">The import context.</param>
        void Import(IManifestSection section, ImportContext importContext);
    }
}
