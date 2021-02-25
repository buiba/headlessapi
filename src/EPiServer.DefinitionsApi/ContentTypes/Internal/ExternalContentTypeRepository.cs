using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.PropertyDataTypes;

namespace EPiServer.DefinitionsApi.ContentTypes.Internal
{
    internal class ExternalContentTypeRepository
    {
        private const int DefaultTake = 100;
        private static readonly ICollection<Guid> UnavailableSystemTypes = new HashSet<Guid> { SystemContentTypes.RootPage, SystemContentTypes.RecycleBin };
        private static readonly ICollection<Guid> ReadOnlySystemTypes = new HashSet<Guid> { SystemContentTypes.RootPage, SystemContentTypes.RecycleBin, SystemContentTypes.ContentAssetFolder, SystemContentTypes.ContentFolder };

        private readonly ContentTypeRepository _internalRepository;
        private readonly ContentTypeMapper _contentTypeMapper;
        private readonly List<IExternalContentTypeServiceExtension> _externalContentTypeServiceExtensions;

        // For mocking
        internal ExternalContentTypeRepository() { }

        public ExternalContentTypeRepository(
            ContentTypeRepository contentTypeRepository,
            ContentTypeMapper contentTypeMapper,
            IEnumerable<IExternalContentTypeServiceExtension> externalContentTypeServiceExtensions)
        {
            _internalRepository = contentTypeRepository ?? throw new ArgumentNullException(nameof(contentTypeRepository));
            _contentTypeMapper = contentTypeMapper ?? throw new ArgumentNullException(nameof(contentTypeMapper));
            _externalContentTypeServiceExtensions = externalContentTypeServiceExtensions?.ToList() ?? throw new ArgumentNullException(nameof(externalContentTypeServiceExtensions));
        }

        public virtual ListResult List(int? take = null) => ListInternal(0, take ?? DefaultTake);

        public virtual ListResult List(ContinuationToken continuationToken) => ListInternal(continuationToken.Skip, continuationToken.Take);

        private ListResult ListInternal(int skip, int take)
        {
            var contentTypes = _internalRepository.List()
                .Where(x => !UnavailableSystemTypes.Contains(x.GUID))
                .Skip(skip)
                .Take(take + 1) // Grab one extra to check for additional items
                .Select(ToExternal)
                .ToArray();

            if (contentTypes.Length > take)
            {
                return new ListResult(new ArraySegment<ExternalContentType>(contentTypes, 0, take), new ContinuationToken(skip + take, take));
            }

            return new ListResult(contentTypes);
        }

        public virtual ExternalContentType Get(Guid id)
        {
            if (id == Guid.Empty)
            {
                return null;
            }

            var contentType = _internalRepository.Load(id);

            if (contentType is null)
            {
                return null;
            }

            return ToExternal(contentType);
        }

        public virtual ExternalContentType Get(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var contentType = _internalRepository.Load(name);

            if (contentType is null)
            {
                return null;
            }

            return ToExternal(contentType);
        }

        public virtual void Save(IEnumerable<ExternalContentType> contentTypes, out IEnumerable<Guid> createdContentTypes, VersionComponent? allowedDowngrades = VersionComponent.None, VersionComponent? allowedUpgrades = null)
        {
            if (contentTypes is null)
            {
                throw new ArgumentNullException(nameof(contentTypes));
            }

            if (contentTypes.FirstOrDefault(x => ReadOnlySystemTypes.Contains(x.Id)) is ExternalContentType systemType)
            {
                throw new ErrorException(HttpStatusCode.BadRequest, $"The id '{systemType.Id}' refers to a system content type. System types are read-only and cannot be updated", ProblemCode.SystemType);
            }

            createdContentTypes = Enumerable.Empty<Guid>();

            var createdContentTypesHash = new HashSet<Guid>();
            var internalContentTypes = new List<ContentType>();
            VersionComponent? calculatedUpgrade = null;
            var shouldAutoIncrement = false;
            var canBeDowngraded = false;

            foreach (var externalContentType in contentTypes)
            {
                var internalContentType = ToInternal(externalContentType, out var previousVersion, out var isNew);
                EnsurePropertyDefinitionTypes(internalContentType, externalContentType, contentTypes);
                shouldAutoIncrement = shouldAutoIncrement || (previousVersion is object && string.IsNullOrWhiteSpace(externalContentType.Version));
                canBeDowngraded = canBeDowngraded || (previousVersion is object && !string.IsNullOrWhiteSpace(externalContentType.Version));
                if (isNew)
                {
                    createdContentTypesHash.Add(internalContentType.GUID);
                }
                else
                {
                    if (!allowedUpgrades.HasValue)
                    {
                        calculatedUpgrade = DetermineUpgrade(calculatedUpgrade, previousVersion, externalContentType.Version);
                    }
                }

                internalContentTypes.Add(internalContentType);
            }

            createdContentTypes = createdContentTypesHash;

            var contentTypeSaveOptions = new ContentTypeSaveOptions
            {
                AllowedUpgrades = allowedUpgrades.HasValue ?
                    allowedUpgrades :
                    calculatedUpgrade ?? VersionComponent.Minor,
                AutoIncrementVersion = shouldAutoIncrement ? true : (bool?)null,
                AllowedDowngrades = canBeDowngraded ? allowedDowngrades : VersionComponent.None
            };

            try
            {
                // call all registered extensions
                foreach (var extension in _externalContentTypeServiceExtensions)
                {
                    extension.Save(contentTypes, internalContentTypes, contentTypeSaveOptions);
                }
            }
            catch
            {
                throw;
            }
        }

        private static void EnsurePropertyDefinitionTypes(ContentType internalContentType, ExternalContentType externalContentType, IEnumerable<ExternalContentType> contentTypes)
        {
            foreach (var property in internalContentType.PropertyDefinitions.Where(p => p.Type is null))
            {
                var externalProperty = externalContentType.Properties.Single(p => p.Name.Equals(property.Name, StringComparison.OrdinalIgnoreCase));
                if (ExternalPropertyDataType.IsBlock(externalProperty.DataType))
                {
                    var blockType = contentTypes.FirstOrDefault(c => nameof(ContentTypeBase.Block).Equals(c.BaseType, StringComparison.OrdinalIgnoreCase) && string.Equals(c.Name, externalProperty.DataType.ItemType, StringComparison.OrdinalIgnoreCase));
                    if (blockType is object)
                    {
                        property.Type = new BlockPropertyDefinitionType
                        {
                            BlockType = new BlockTypeReference { GUID = blockType.Id, Name = blockType.Name }
                        };
                        continue;
                    }
                }

                throw new ErrorException(HttpStatusCode.BadRequest, $"Could not resolve property type '{externalProperty.DataType}' for property '{property.Name}' on Content Type '{internalContentType.Name}'.");
            }
        }

        public virtual bool TryDelete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return false;
            }

            if (ReadOnlySystemTypes.Contains(id))
            {
                throw new ErrorException(HttpStatusCode.BadRequest, $"The id '{id}' refers to a system content type. System types are read-only and cannot be deleted", ProblemCode.SystemType);
            }

            var deleted = false;

            // call all registered extensions
            foreach (var extension in _externalContentTypeServiceExtensions)
            {
                try
                {
                    if (extension.TryDelete(id))
                    {
                        deleted = true;
                        break;
                    }
                }
                catch (DataAbstractionException ex)
                {
                    throw new ErrorException(HttpStatusCode.Conflict, ex.Message, ProblemCode.InUse);
                }
            }

            return deleted;
        }

        private ExternalContentType ToExternal(ContentType contentType)
        {
            var external = new ExternalContentType();
            _contentTypeMapper.MapToExternal(contentType, external);
            return external;
        }

        private ContentType ToInternal(ExternalContentType externalContentType, out Version previousVersion, out bool isNew)
        {
            isNew = false;
            var internalContentType = Load(externalContentType);
            previousVersion = internalContentType?.Version;

            if (internalContentType is null)
            {
                switch (externalContentType.BaseType)
                {
                    case nameof(ContentTypeBase.Block):
                        internalContentType = new BlockType();
                        break;
                    case nameof(ContentTypeBase.Page):
                        internalContentType = new PageType();
                        break;
                    default:
                        internalContentType = new ContentType();
                        break;
                }

                internalContentType.GUID = externalContentType.Id;
                isNew = true;
            }

            _contentTypeMapper.MapToInternal(externalContentType, internalContentType);

            return internalContentType;
        }

        private ContentType Load(ExternalContentType externalContentType)
        {
            if (externalContentType is null)
            {
                throw new ArgumentNullException(nameof(externalContentType));
            }

            ContentType internalContentType;
            if (externalContentType.Id == Guid.Empty)
            {
                internalContentType = _internalRepository.Load(externalContentType.Name)?.CreateWritableClone() as ContentType;
            }
            else
            {
                internalContentType = _internalRepository.Load(externalContentType.Id)?.CreateWritableClone() as ContentType;
            }

            return internalContentType;
        }

        private static VersionComponent? DetermineUpgrade(VersionComponent? calculatedUpgrade, Version existingVersion, string updatedVersionString)
        {
            if (existingVersion != null && !string.IsNullOrWhiteSpace(updatedVersionString))
            {
                var updatedVersion = new Version(updatedVersionString);
                var diff = Diff(existingVersion, updatedVersion);
                return calculatedUpgrade.HasValue ? calculatedUpgrade.Value > diff ? calculatedUpgrade : diff : diff;
            }

            return calculatedUpgrade;
        }

        private static VersionComponent Diff(Version from, Version to)
        {
            if (from.Major != to.Major)
            {
                return VersionComponent.Major;
            }

            if (from.Minor != to.Minor)
            {
                return VersionComponent.Minor;
            }

            if (from.Build != to.Build || from.Revision != to.Revision)
            {
                return VersionComponent.Patch;
            }

            return VersionComponent.None;
        }
    }
}
