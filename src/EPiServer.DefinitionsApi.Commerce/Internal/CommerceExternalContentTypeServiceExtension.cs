using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Provider;
using EPiServer.Commerce.Catalog.Provider.Construction;
using EPiServer.Commerce.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAbstraction.RuntimeModel;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Extensions;
using Mediachase.MetaDataPlus;
using Mediachase.MetaDataPlus.Configurator;
using EPiServer.DefinitionsApi.ContentTypes.Internal;
using EPiServer.DefinitionsApi.ContentTypes;
using EPiServer.ContentApi.Error.Internal;

namespace EPiServer.DefinitionsApi.Commerce.Internal
{
    internal class CommerceExternalContentTypeServiceExtension : IExternalContentTypeServiceExtension
    {
        private readonly IEnumerable<Type> _excludedBaseTypes = new Type[] { typeof(CatalogContent), typeof(RootContent) };

        private const string MetaClassNamespace = "Mediachase.Commerce.Catalog.User";
        private const string MetaFieldNamespace = "Mediachase.Commerce.Catalog";
        private const string NodeMetaClassName = "CatalogNode";
        private const string EntryMetaClassName = "CatalogEntry";
        private const string BlockPropertyDivider = "_";
        private const string BlockPropertyPrefix = "EPiBlock";

        private readonly ContentTypeRepository _internalRepository;
        private readonly IContentTypeBaseProvider _contentTypeBaseProvider;
        private readonly IContentTypeModelAssigner _contentTypeModelAssigner;

        private MetaDataContext _metaDataContext;
        private readonly MetaDataTypeResolver _metaDataTypeResolver;
        private readonly MetaDataPropertyResolver _metaDataPropertyResolver;
        private readonly IPropertyDefinitionTypeRepository _propertyDefinitionTypeRepository;
        private IEnumerable<IContentTypeModelScanner> _contentTypeModelScanners;

        [NonSerialized]
        private static readonly ILogger Log = LogManager.GetLogger(typeof(CommerceExternalContentTypeServiceExtension));

        private readonly bool _ignoreMetaFieldMismatch;
        private const string IgnoreMetafieldMismatchWarning = "To ignore those mismatches, add a setting with name \"episerver.commerce:IgnorePropertyAndMetafieldMisMatch\", and value to true. However, proceed with caution! ";

        private IEnumerable<IContentTypeModelScanner> ContentTypeModelScanners
        {
            get => _contentTypeModelScanners ??
                   (_contentTypeModelScanners = ServiceLocator.Current.GetAllInstances<IContentTypeModelScanner>());
            set => _contentTypeModelScanners = value;
        }

        private MetaDataContext MetaDataContext
        {
            get => _metaDataContext ?? (_metaDataContext = CatalogContext.MetaDataContext);
            set => _metaDataContext = value;
        }

        // For mocking
        internal CommerceExternalContentTypeServiceExtension(ContentTypeRepository contentTypeRepository)
        {
            _internalRepository = contentTypeRepository ?? throw new ArgumentNullException(nameof(contentTypeRepository));
            _contentTypeBaseProvider = new CommerceContentTypeBaseProvider();
        }

        public CommerceExternalContentTypeServiceExtension(
            ContentTypeRepository contentTypeRepository,
            CommerceContentTypeBaseProvider commerceContentTypeBaseProvider,
            IContentTypeModelAssigner contentTypeModelAssigner,
            MetaDataTypeResolver metaDataTypeResolver,
            MetaDataPropertyResolver metaDataPropertyResolver,
            IPropertyDefinitionTypeRepository propertyDefinitionTypeRepository)
        {
            _internalRepository = contentTypeRepository ?? throw new ArgumentNullException(nameof(contentTypeRepository));
            _contentTypeBaseProvider = commerceContentTypeBaseProvider ?? new CommerceContentTypeBaseProvider();
            _contentTypeModelAssigner = contentTypeModelAssigner ?? throw new ArgumentNullException(nameof(contentTypeModelAssigner));
            _metaDataTypeResolver = metaDataTypeResolver ?? throw new ArgumentNullException(nameof(metaDataTypeResolver));
            _metaDataPropertyResolver = metaDataPropertyResolver ?? throw new ArgumentNullException(nameof(metaDataPropertyResolver));
            _propertyDefinitionTypeRepository = propertyDefinitionTypeRepository ?? throw new ArgumentNullException(nameof(propertyDefinitionTypeRepository));

            var ignoreMetaFieldMismatchSetting =
                ConfigurationManager.AppSettings["episerver.commerce:IgnorePropertyAndMetafieldMisMatch"];
            _ignoreMetaFieldMismatch = bool.TryParse(ignoreMetaFieldMismatchSetting, out _ignoreMetaFieldMismatch) &&
                                       _ignoreMetaFieldMismatch;
        }

        public void Save(IEnumerable<ExternalContentType> externalContentTypes, IEnumerable<ContentType> internalContentTypes, ContentTypeSaveOptions contentTypeSaveOptions)
        {
            var commerceInternalContentTypes = internalContentTypes.Where(x => CanHandle(x)).ToList();

            if (commerceInternalContentTypes.Any())
            {
                // save commerce content types
                SaveCommerceContentTypes(commerceInternalContentTypes, contentTypeSaveOptions);
            }
        }

        public bool TryDelete(Guid id)
        {
            var contentType = _internalRepository.Load(id);

            if (contentType is object && CanHandle(contentType))
            {
                // if MetaClass, delete from commerce database too
                if (IsMetaClass(contentType))
                {
                    DeleteMetaClass(contentType);
                }

                _internalRepository.Delete(contentType);
                return true;
            }

            return false;
        }

        private bool CanHandle(ContentType contentType) => _contentTypeBaseProvider.Resolve(contentType.Base) is null ? false : true;

        private bool IsMetaClass(ContentType contentType) => IsMetaClass(_contentTypeBaseProvider.Resolve(contentType.Base));

        private bool IsMetaClass(Type baseType) => typeof(IMetaClass).IsAssignableFrom(baseType) && !_excludedBaseTypes.Any(p => p == baseType);

        internal virtual void SaveCommerceContentTypes(IEnumerable<ContentType> internalContentTypes, ContentTypeSaveOptions contentTypeSaveOptions)
        {
            foreach (var contentType in internalContentTypes)
            {
                if (!CanHandle(contentType))
                {
                    continue;
                }

                try
                {
                    _internalRepository.Save(new[] { contentType }, contentTypeSaveOptions);

                    // if MetaClass, save to commerce database too
                    if (IsMetaClass(contentType))
                    {
                        SaveMetaClass(contentType);
                        SaveMetaField(contentType);
                    }
                }
                catch (InvalidContentTypeBaseException ex)
                {
                    throw new ErrorException(HttpStatusCode.Conflict, ex.Message, ProblemCode.InvalidBase);
                }
                catch (ConflictingResourceException ex)
                {
                    throw new ErrorException(HttpStatusCode.Conflict, ex.Message);
                }
                catch (VersionValidationException ex)
                {
                    throw new ErrorException(HttpStatusCode.Conflict, ex.Message, ProblemCode.Version);
                }
            }
        }

        internal virtual void DeleteMetaClass(ContentType contentType)
        {
            var metaClassName = GetMetaClassName(contentType);
            var metaClass = MetaClass.Load(MetaDataContext, metaClassName);
            if (metaClass is object)
            {
                MetaClass.Delete(MetaDataContext, metaClass.Id);
            }
        }

        private static string GetMetaClassName(ContentType contentType) => contentType.Name;

        private bool MetaClassExists(string metaClassName) => MetaClass.Load(MetaDataContext, metaClassName) != null;

        internal virtual void SaveMetaClass(ContentType contentType)
        {
            var metaClassName = GetMetaClassName(contentType);
            if (MetaClassExists(metaClassName))
            {
                UpdateMetaClass(contentType, metaClassName);
            }
            else
            {
                CreateMetaClass(contentType, metaClassName);
            }
        }

        internal virtual void SaveMetaField(ContentType contentType)
        {
            var metaClassName = GetMetaClassName(contentType);

            var metaClass = MetaClass.Load(MetaDataContext, metaClassName);
            if (metaClass == null)
            {
                return;
            }

            var removedMetaFields = metaClass.MetaFields.Where(p => !p.IsSystem && !p.Name.StartsWith("Epi", StringComparison.OrdinalIgnoreCase) && !contentType.PropertyDefinitions.Any(e => string.Equals(e.Name, p.Name, StringComparison.OrdinalIgnoreCase))).ToList();
            foreach (var metaField in removedMetaFields)
            {
                metaClass.DeleteField(metaField);

                if (MetaField.Load(MetaDataContext, metaField.Id).OwnerMetaClassIdList.Count <= 0)
                {
                    MetaField.Delete(MetaDataContext, metaField.Id);
                }
            }

            AssignModelPropertiesToMetaClass(metaClass, contentType.PropertyDefinitions, _contentTypeBaseProvider.Resolve(contentType.Base));
        }

        private void CreateMetaClass(ContentType contentType, string metaClassName)
        {
            var parentMetaClass = MetaClass.Load(MetaDataContext, typeof(EntryContentBase).IsAssignableFrom(_contentTypeBaseProvider.Resolve(contentType.Base)) ? EntryMetaClassName : NodeMetaClassName);

            MetaClass.Create(MetaDataContext,
                             MetaClassNamespace,
                             metaClassName,
                             contentType.Name ?? metaClassName,
                             string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", parentMetaClass.Name, "Ex_", metaClassName),
                             parentMetaClass.Id,
                             false,
                             contentType.Name);
        }

        private void UpdateMetaClass(ContentType contentType, string metaClassName)
        {
            var metaClass = MetaClass.Load(MetaDataContext, metaClassName);

            if (!string.IsNullOrEmpty(contentType.Name) && contentType.Name != metaClass.FriendlyName)
            {
                metaClass.FriendlyName = contentType.Name;
            }

            if (contentType.Name != null && contentType.Name != metaClass.Description)
            {
                metaClass.Description = contentType.Name;
            }
        }

        private void AssignModelPropertiesToMetaClass(MetaClass metaClass, PropertyDefinitionCollection properties, Type baseType)
        {
            foreach (var property in properties)
            {
                var propertyInfo = baseType.GetProperty(property.Name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (propertyInfo is object && propertyInfo.GetCustomAttributes(typeof(IgnoreMetaDataPlusSynchronizationAttribute)).Any())
                {
                    continue;
                }

                var propertyDefinitionModel = ToPropertyDefinitionModel(property);

                if (typeof(IContentData).IsAssignableFrom(propertyDefinitionModel.Type))
                {
                    var propertyModel = new PropertyDefinitionModel()
                    {
                        Name = $"{BlockPropertyPrefix}{BlockPropertyDivider}{propertyDefinitionModel.Name}",
                        Type = propertyDefinitionModel.Type
                    };

                    RecursiveCreateOrUpdateMetaField(metaClass, propertyModel);
                }
                else
                {
                    CreateOrUpdateMetaField(metaClass, propertyDefinitionModel);
                }
            }
        }

        private PropertyDefinitionModel ToPropertyDefinitionModel(PropertyDefinition property)
        {
            return new PropertyDefinitionModel()
            {
                Name = property.Name,
                DisplayName = property.Name,
                Description = property.Name,
                Type = property.Type.DefinitionType,
                CultureSpecific = property.LanguageSpecific
            };
        }

        private void RecursiveCreateOrUpdateMetaField(MetaClass metaClass, PropertyDefinitionModel propertyDefinitionModel)
        {
            if (Log.IsDebugEnabled())
            {
                Log.DebugBeginMethod(nameof(RecursiveCreateOrUpdateMetaField), metaClass, propertyDefinitionModel);
            }

            var contentTypeModel = new ContentTypeModel()
            {
                Name = propertyDefinitionModel.Type.Name,
                ModelType = propertyDefinitionModel.Type
            };

            foreach (var contentTypeModelScanner in ContentTypeModelScanners)
            {
                foreach (var property in contentTypeModelScanner.GetProperties(contentTypeModel))
                {
                    var propertyModel = new PropertyDefinitionModel()
                    {
                        Name = string.Join(BlockPropertyDivider, propertyDefinitionModel.Name, property.Name),
                        Type = property.PropertyType
                    };

                    _contentTypeModelAssigner.AssignValuesToPropertyDefinition(propertyModel, property, contentTypeModel);

                    if (typeof(IContentData).IsAssignableFrom(property.PropertyType))
                    {
                        RecursiveCreateOrUpdateMetaField(metaClass, propertyModel);
                    }
                    else
                    {
                        CreateOrUpdateMetaField(metaClass, propertyModel);
                    }
                }
            }
        }

        private void CreateOrUpdateMetaField(MetaClass metaClass, PropertyDefinitionModel propertyDefinitionModel)
        {
            if (Log.IsDebugEnabled())
            {
                Log.DebugBeginMethod(nameof(CreateOrUpdateMetaField), metaClass, propertyDefinitionModel);
            }

            MetaDataType metaDataType;
            if (propertyDefinitionModel.BackingType != null)
            {
                var propertyDefinitionType = _propertyDefinitionTypeRepository.Load(propertyDefinitionModel.BackingType);

                if (propertyDefinitionType == null)
                {
                    throw new InvalidOperationException(
                        $"The property {propertyDefinitionModel.Name} has a backing type {propertyDefinitionModel.BackingType.FullName} which does not exist in the property definition type repository.");
                }

                var metaDataTypeFromBackingType = _metaDataTypeResolver.GetMetaDataTypeFromBackingType(propertyDefinitionModel.BackingType);
                if (metaDataTypeFromBackingType.HasValue)
                {
                    metaDataType = metaDataTypeFromBackingType.Value;
                }
                else
                {
                    metaDataType = _metaDataTypeResolver.GetMetaDataType(propertyDefinitionType.DataType);
                }
            }
            else
            {
                var propertyDefinitionType = _propertyDefinitionTypeRepository.Load(propertyDefinitionModel.Type);
                metaDataType = propertyDefinitionType != null ? _metaDataTypeResolver.GetMetaDataType(propertyDefinitionType.DataType) : _metaDataTypeResolver.GetMetaDataType(propertyDefinitionModel.Type);
            }

            if (MetaFieldExistsOfOtherType(propertyDefinitionModel.Name, metaDataType, out var metaField))
            {
                if (_ignoreMetaFieldMismatch)
                {
                    Log.Error($"A meta field with the same name as the property '{propertyDefinitionModel.Name}' exists, but has another data type. " +
                             $"Property type: {metaDataType}. Meta field type: {metaField.DataType}." +
                             "A [BackingType] attribute with proper type is probably missing." +
                             "The site is configured to ignore this error with episerver.commerce:IgnorePropertyAndMetafieldMisMatch setting.");
                }
                else
                {
                    throw new InvalidOperationException(
                        $"A meta field with the same name as the property '{propertyDefinitionModel.Name}' exists, but has another data type. " +
                        $"Property type: {metaDataType}. Meta field type: {metaField.DataType}." +
                        "A [BackingType] attribute with proper type is probably missing." +
                        IgnoreMetafieldMismatchWarning
                        );
                }
            }

            if (metaField == null)
            {
                metaField = CreateMetaField(propertyDefinitionModel, metaDataType);
            }
            else
            {
                UpdateMetaField(propertyDefinitionModel, metaField);
            }

            if (!metaClass.MetaFields.Contains(metaField))
            {
                metaClass.AddField(metaField);
            }
        }

        private MetaField CreateMetaField(PropertyDefinitionModel propertyDefinitionModel, MetaDataType metaDataType)
        {
            if (Log.IsDebugEnabled())
            {
                Log.DebugBeginMethod(nameof(CreateMetaField), propertyDefinitionModel, metaDataType);
            }

            var metaField = MetaField.Create(
                MetaDataContext,
                MetaFieldNamespace,
                propertyDefinitionModel.Name,
                propertyDefinitionModel.DisplayName ?? propertyDefinitionModel.Name,
                propertyDefinitionModel.Description,
                metaDataType,
                0, // Length
                !propertyDefinitionModel.Required.GetValueOrDefault(),
                propertyDefinitionModel.CultureSpecific.GetValueOrDefault(),
                propertyDefinitionModel.Searchable.GetValueOrDefault(),
                false); //encrypted

            return metaField;
        }

        private void UpdateMetaField(PropertyDefinitionModel propertyDefinitionModel, MetaField metaField)
        {
            if (Log.IsDebugEnabled())
            {
                Log.DebugBeginMethod(nameof(UpdateMetaField), propertyDefinitionModel, metaField);
            }

            if (!string.IsNullOrEmpty(propertyDefinitionModel.DisplayName))
            {
                metaField.FriendlyName = propertyDefinitionModel.DisplayName;
            }

            if (propertyDefinitionModel.Description != null)
            {
                metaField.Description = propertyDefinitionModel.Description;
            }

            var allowNulls = !propertyDefinitionModel.Required.GetValueOrDefault();
            try
            {
                metaField.SetAllowNulls(allowNulls);
            }
            catch (InvalidOperationException x)
            {
                Log.Error($"Failed to update the 'Required' option of the property '{propertyDefinitionModel.Name}': {x.Message}");
            }

            var multiLanguageValue = propertyDefinitionModel.CultureSpecific.GetValueOrDefault();
            metaField.MultiLanguageValue = multiLanguageValue;

            var allowSearch = propertyDefinitionModel.Searchable.GetValueOrDefault();
            metaField.SetAllowSearch(allowSearch);
        }

        private bool MetaFieldExistsOfOtherType(string metaFieldName, MetaDataType metaDataType, out MetaField metaField)
        {
            metaField = MetaField.Load(MetaDataContext, metaFieldName);
            if (metaField == null)
            {
                return false;
            }

            return !IsValid(metaDataType, metaField.DataType);
        }

        /// <summary>
        /// Validates if the two types are compatible.
        /// </summary>
        /// <param name="typeFromCode">The property type created from content data model.</param>
        /// <param name="typeFromMetaData">The property type in Meta data plus.</param>
        /// <returns><c>True</c> if the types are compatible, otherwise <c>false</c>.</returns>
        private bool IsValid(MetaDataType typeFromCode, MetaDataType typeFromMetaData)
        {
            if (typeFromCode == typeFromMetaData)
            {
                return true;
            }

            switch (typeFromCode)
            {
                case MetaDataType.LongString:
                {
                    return typeFromMetaData == MetaDataType.ShortString ||
                           typeFromMetaData == MetaDataType.Text;
                }
                case MetaDataType.Integer:
                {
                    return typeFromMetaData == MetaDataType.SmallInt ||
                           typeFromMetaData == MetaDataType.TinyInt ||
                           typeFromMetaData == MetaDataType.Int;
                }
                case MetaDataType.DateTime:
                {
                    return typeFromMetaData == MetaDataType.Date ||
                           typeFromMetaData == MetaDataType.SmallDateTime;
                }
                case MetaDataType.EnumSingleValue:
                {
                    return typeFromMetaData == MetaDataType.DictionarySingleValue;
                }
                case MetaDataType.EnumMultiValue:
                {
                    return typeFromMetaData == MetaDataType.DictionaryMultiValue;
                }
            }

            return false;
        }

    }
}
