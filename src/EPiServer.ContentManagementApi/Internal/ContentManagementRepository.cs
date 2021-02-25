using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.Core;
using EPiServer.Web;
using EPiServer.DataAbstraction;
using EPiServer.Web.Routing;
using EPiServer.ContentManagementApi.Models.Internal;
using EPiServer.DataAccess;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Data.Entity;
using System.Collections.Generic;
using System.Data.SqlClient;
using EPiServer.ContentManagementApi.Serialization;

namespace EPiServer.ContentManagementApi.Internal
{
    internal class ContentManagementRepository
    {
        private readonly IContentRepository _contentRepository;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly IPropertyDataValueConverterResolver _propertyDataValueConverterResolver;
        private readonly IContentVersionRepository _contentVersionRepository;
        private readonly IPermanentLinkMapper _permanentLinkMapper;
        private readonly ISiteDefinitionRepository _siteDefinitionRepository;
        private readonly IContentLoader _contentLoader;
        private readonly RequiredRoleEvaluator _requiredRoleEvaluator;
        private readonly IStatusTransitionEvaluator _statusTransitionEvaluator;

        public ContentManagementRepository(
            IContentRepository contentRepository,
            IContentVersionRepository contentVersionRepository,
            IPermanentLinkMapper permanentLinkMapper,
            ISiteDefinitionRepository siteDefinitionRepository,
            IContentTypeRepository contentTypeRepository,
            IPropertyDataValueConverterResolver propertyDataValueConverterResolver,
            IContentLoader contentLoader,
            RequiredRoleEvaluator requiredRoleEvaluator,
            IStatusTransitionEvaluator statusTransitionEvaluator)
        {
            _contentRepository = contentRepository ?? throw new ArgumentNullException(nameof(contentRepository));
            _siteDefinitionRepository = siteDefinitionRepository ?? throw new ArgumentNullException(nameof(siteDefinitionRepository));
            _contentVersionRepository = contentVersionRepository ?? throw new ArgumentNullException(nameof(contentVersionRepository));
            _permanentLinkMapper = permanentLinkMapper ?? throw new ArgumentNullException(nameof(permanentLinkMapper));
            _contentTypeRepository = contentTypeRepository ?? throw new ArgumentNullException(nameof(contentTypeRepository));
            _propertyDataValueConverterResolver = propertyDataValueConverterResolver ?? throw new ArgumentNullException(nameof(propertyDataValueConverterResolver));
            _contentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
            _requiredRoleEvaluator = requiredRoleEvaluator ?? throw new ArgumentNullException(nameof(requiredRoleEvaluator));
            _statusTransitionEvaluator = statusTransitionEvaluator;
        }

        public IContent GetCommonDraft(Guid contentGuid, string language)
        {
            var permanentLinkMap = _permanentLinkMapper.Find(contentGuid);
            if (permanentLinkMap is null)
            {
                throw new ErrorException(HttpStatusCode.NotFound, $"Content with guid {contentGuid} was not found");
            }

            return GetCommonDraft(permanentLinkMap.ContentReference, language);
        }

        /// <summary>
        ///     Get common draft version of a content. If language is null, the master language content is returned.
        /// </summary>
        public IContent GetCommonDraft(ContentReference contentReference, string language)
        {
            try
            {
                var requestedLanguage = language;
                var content = _contentLoader.Get<IContent>(contentReference);

                EvaluateRequiredRole(content);

                if (string.IsNullOrWhiteSpace(language))
                {
                    requestedLanguage = (content as ILocalizable)?.MasterLanguage?.Name;
                }

                var commonDraft = _contentVersionRepository.LoadCommonDraft(contentReference, requestedLanguage);
                return commonDraft is null ? null : _contentRepository.Get<IContent>(commonDraft.ContentLink);
            }
            catch (ContentNotFoundException ex)
            {
                throw new ErrorException(HttpStatusCode.NotFound, ex.Message);
            }
        }

        public bool TryDelete(ContentReference contentReference, bool permanentDelete)
        {
            var result = _contentRepository.TryGet<IContent>(contentReference, out var content);
            if (!result)
            {
                return false;
            }

            return Delete(content, permanentDelete);
        }

        /// If <paramref name="permanentDelete"/>"/> is set to true in the header, should lead to content being deleted directly without moving to the recycle bin.
        /// If <paramref name="permanentDelete"/>"/> is set to false in the header when deleting a content item in the recycle bin, should prevent the item from being permanently deleted.
        public bool TryDelete(Guid contentGuid, bool permanentDelete)
        {
            var result = _contentRepository.TryGet<IContent>(contentGuid, out var content);
            if (!result)
            {
                return false;
            }

            return Delete(content, permanentDelete);
        }

        /// <summary>
        /// Moves content from its current location to another location.
        /// </summary>
        /// <param name="contentGuid">Guid of content that should be moved.</param>
        /// <param name="parentLink">The new location where the content will be moved as a child to.</param>
        public bool Move(Guid contentGuid, ContentReferenceInputModel parentLink)
        {
            if (!_contentRepository.TryGet<IContent>(contentGuid, out var sourceContent))
            {
                return false;
            }

            return MoveInternal(sourceContent, parentLink);
        }

        /// <summary>
        /// Moves content from its current location to another location.
        /// </summary>
        /// <param name="contentReference">Content reference of content that should be moved.</param>
        /// <param name="parentLink">The new location where the content will be moved as a child to.</param>
        public bool Move(ContentReference contentReference, ContentReferenceInputModel parentLink)
        {
            if (!_contentRepository.TryGet<IContent>(contentReference, out var sourceContent))
            {
                return false;
            }

            return MoveInternal(sourceContent, parentLink);
        }

        public ContentReference Create(ContentApiCreateModel inputContentModel, SaveContentOptions saveContentOptions)
        {
            if (inputContentModel is null)
            {
                throw new ArgumentNullException(nameof(inputContentModel));
            }

            // Get parent content
            if (!TryGetContent(inputContentModel.ParentLink, out var parent))
            {
                throw new ErrorException(HttpStatusCode.BadRequest, $"Cannot get parent content", ProblemCode.InvalidParent);
            }

            EvaluateRequiredRole(parent);

            // Get content type
            var contentTypeName = inputContentModel.ContentType.Last();
            var contentType = _contentTypeRepository.Load(contentTypeName);

            var defaultContent = _contentRepository.GetDefault<IContent>(parent.ContentLink, contentType.ID, !string.IsNullOrWhiteSpace(inputContentModel.Language?.Name) ? new CultureInfo(inputContentModel.Language?.Name) : null);

            ValidateLocalizableContent(defaultContent, inputContentModel.Language);

            if (inputContentModel.ContentLink is object && inputContentModel.ContentLink.GuidValue.HasValue && inputContentModel.ContentLink.GuidValue.Value != Guid.Empty)
            {
                defaultContent.ContentGuid = inputContentModel.ContentLink.GuidValue.Value;
            }

            return Save(inputContentModel, defaultContent, saveContentOptions);
        }

        public ContentReference Update(Guid contentGuid, ContentApiCreateModel inputContentModel, SaveContentOptions saveContentOptions)
        {
            if (inputContentModel is null)
            {
                throw new ArgumentNullException(nameof(inputContentModel));
            }

            var content = GetCommonDraft(contentGuid, inputContentModel.Language?.Name);

            if (content is null)
            {
                throw new ErrorException(HttpStatusCode.NotFound, $"The content with id '{contentGuid}' does not exist.");
            }

            if (content is ILocalizable locale)
            {

                if (!locale.MasterLanguage.Name.Equals(inputContentModel.Language?.Name, StringComparison.OrdinalIgnoreCase)
                    && HasNonBranchSpecificProperty(inputContentModel.Properties, content.Property, out var nonBranchSpecificPropertyName))
                {
                    throw new ErrorException(HttpStatusCode.BadRequest, $"Cannot provide non-branch specific property '{nonBranchSpecificPropertyName}' when not passing the master language");
                }

                if (inputContentModel.Language is object
                    && !locale.ExistingLanguages.Select(l => l.Name).Contains(inputContentModel.Language.Name))
                {
                    throw new ErrorException(HttpStatusCode.BadRequest, $"The provided language does not exist for content with id '{contentGuid}'.");
                }
            }

            ValidateLocalizableContent(content, inputContentModel.Language);

            content = content is IReadOnly readOnly ? (IContent)readOnly.CreateWritableClone() : content;

            content.Name = inputContentModel.Name;
            AssignRoutableContent(content, inputContentModel.RouteSegment);

            AssignVersionableContent(content, inputContentModel);
            AssignProperties(content, inputContentModel.Properties, true);

            var saveAction = GetSaveAction(inputContentModel.Status, saveContentOptions.ContentValidationMode);

            ValidateStatusTransition(content, inputContentModel.Status, saveAction);
            ContentReference save() => _contentRepository.Save(content, saveAction);

            var savedContentLink = HandleError(save);
            _contentVersionRepository.SetCommonDraft(savedContentLink);

            return savedContentLink;
        }

        public ContentReference CreateVersion(Guid contentGuid, ContentApiCreateModel inputContentModel, SaveContentOptions saveContentOptions)
        {
            var permanentLinkMap = _permanentLinkMapper.Find(contentGuid);
            if (permanentLinkMap is null)
            {
                throw new ErrorException(HttpStatusCode.NotFound, $"The provided content guid {contentGuid} does not exist.");
            }

            return CreateVersion(permanentLinkMap.ContentReference, inputContentModel, saveContentOptions);
        }

        public ContentReference CreateVersion(ContentReference contentLink, ContentApiCreateModel inputContentModel, SaveContentOptions saveContentOptions)
        {
            if (ContentReference.IsNullOrEmpty(contentLink))
            {
                throw new ArgumentNullException(nameof(contentLink));
            }

            if (inputContentModel is null)
            {
                throw new ArgumentNullException(nameof(inputContentModel));
            }

            var content = GetCommonDraft(contentLink, inputContentModel.Language?.Name);
            if (content is null)
            {
                content = _contentRepository.CreateLanguageBranch<IContent>(contentLink, CultureInfo.GetCultureInfo(inputContentModel.Language?.Name));
            }

            if (!(content is IVersionable))
            {
                throw new ErrorException(HttpStatusCode.BadRequest, "Cannot create new version of non-versionable content", ProblemCode.ContentNotVersionable);
            }

            ValidateLocalizableContent(content, inputContentModel.Language);

            if (content is ILocalizable locale
                && !locale.MasterLanguage.Name.Equals(inputContentModel.Language.Name, StringComparison.OrdinalIgnoreCase)
                && HasNonBranchSpecificProperty(inputContentModel.Properties, content.Property, out var nonBranchSpecificPropertyName))
            {
                throw new ErrorException(HttpStatusCode.BadRequest, $"Cannot provide non-branch specific property '{nonBranchSpecificPropertyName}' when not passing the master language");
            }

            var writableContent = content is IReadOnly readOnly ? (IContent)readOnly.CreateWritableClone() : content;

            var savedContentLink = Save(inputContentModel, writableContent, saveContentOptions);
            _contentVersionRepository.SetCommonDraft(savedContentLink);

            return savedContentLink;
        }

        public ContentReference Patch(Guid contentGuid, ContentApiPatchModel patchModel, SaveContentOptions saveContentOptions)
        {
            var permanentLinkMap = _permanentLinkMapper.Find(contentGuid);
            if (permanentLinkMap is null)
            {
                throw new ErrorException(HttpStatusCode.NotFound, $"No content with the provided content guid {contentGuid} exists.");
            }

            return Patch(permanentLinkMap.ContentReference, patchModel, saveContentOptions);
        }

        public ContentReference Patch(ContentReference contentLink, ContentApiPatchModel patchModel, SaveContentOptions saveContentOptions)
        {
            if (ContentReference.IsNullOrEmpty(contentLink))
            {
                throw new ArgumentNullException(nameof(contentLink));
            }

            if (patchModel is null)
            {
                throw new ArgumentNullException(nameof(patchModel));
            }

            var content = GetCommonDraft(contentLink, patchModel.Language?.Name);
            if (content is null)
            {
                throw new ErrorException(HttpStatusCode.NotFound, $"The content with id '{contentLink}' does not exist.");
            }

            if (!(content is ILocalizable) && patchModel.Language is object)
            {
                throw new ErrorException(HttpStatusCode.BadRequest, $"Language cannot be set when content type is not localizable", ProblemCode.ContentNotLocalizable);
            }

            if (content is ILocalizable localeContent && patchModel.Language is object && !localeContent.ExistingLanguages.Select(l => l.Name).Contains(patchModel.Language.Name))
            {
                throw new ErrorException(HttpStatusCode.BadRequest, $"The provided language does not exist for content with id '{contentLink}'.");
            }

            if (content is ILocalizable locale
                 && !locale.MasterLanguage.Name.Equals(patchModel.Language?.Name, StringComparison.OrdinalIgnoreCase)
                 && HasNonBranchSpecificProperty(patchModel.Properties, content.Property, out var nonBranchSpecificPropertyName))
            {
                throw new ErrorException(HttpStatusCode.BadRequest, $"Cannot provide non-branch specific property '{nonBranchSpecificPropertyName}' when not passing the master language");
            }

            var writableContent = content is IReadOnly readOnly ? (IContent)readOnly.CreateWritableClone() : content;

            if (!string.IsNullOrWhiteSpace(patchModel.Name))
            {
                writableContent.Name = patchModel.Name;
            }
           
            if (patchModel.UpdatedMetadata.Contains(nameof(ContentApiPatchModel.RouteSegment)))
            {
                AssignRoutableContent(writableContent, patchModel.RouteSegment);
            }

            AssignVersionableContent(writableContent, patchModel);
            AssignProperties(writableContent, patchModel.Properties, false);

            var saveAction = SaveAction.ForceCurrentVersion | GetSaveAction(patchModel.Status, saveContentOptions.ContentValidationMode);

            ValidateStatusTransition(writableContent, patchModel.Status, saveAction);
            ContentReference save() => _contentRepository.Save(writableContent, saveAction);

            return HandleError(save);
        }

        private bool HasNonBranchSpecificProperty(IDictionary<string, object> properties, PropertyDataCollection propertyDataCollection, out string nonBranchSpecificPropertyName)
        {
            nonBranchSpecificPropertyName = null;

            var nonBranchSpecificProperty = propertyDataCollection.FirstOrDefault(p => properties.ContainsKey(p.Name) && !p.IsLanguageSpecific);
            if (nonBranchSpecificProperty is object)
            {
                nonBranchSpecificPropertyName = nonBranchSpecificProperty.Name;
                return true;
            }

            return false;
        }

        internal ContentReference Save(ContentApiCreateModel inputContentModel, IContent content, SaveContentOptions saveContentOptions)
        {
            content.Name = inputContentModel.Name;

            AssignRoutableContent(content, inputContentModel.RouteSegment);
            AssignVersionableContent(content, inputContentModel);

            AssignProperties(content, inputContentModel.Properties, true);

            var saveAction = GetSaveAction(inputContentModel.Status, saveContentOptions.ContentValidationMode);

            ValidateStatusTransition(content, inputContentModel.Status, saveAction);

            ContentReference save() => _contentRepository.Save(content, saveAction | SaveAction.ForceNewVersion );

            return HandleError(save);
        }

        /// <summary>
        /// Expose for test
        /// </summary>
        internal void ValidateLocalizableContent(IContent content, LanguageModel language)
        {
            if (content is ILocalizable && language is null)
            {
                throw new ErrorException(HttpStatusCode.BadRequest, $"Language should not be null when content type is localizable");
            }

            if (!(content is ILocalizable) && language is object)
            {
                throw new ErrorException(HttpStatusCode.BadRequest, $"Language cannot be set when content type is not localizable", ProblemCode.ContentNotLocalizable);
            }
        }

        private void ValidateStatusTransition(IContent content, VersionStatus? status, SaveAction saveAction)
        {
            var transition = _statusTransitionEvaluator.Evaluate(content, saveAction);
            if (transition == StatusTransition.Invalid)
            {
                throw new ErrorException(HttpStatusCode.BadRequest, $"A content item cannot be created with status '{status}'.");
            }

            if (transition.NextStatus == VersionStatus.DelayedPublish && (content as IVersionable)?.StartPublish == null)
            {
                throw new ErrorException(HttpStatusCode.BadRequest, $"StartPublish must be set when content item is set for scheduled publishing");
            }
        }

        internal T HandleError<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (AccessDeniedException ex)
            {
                throw new ErrorException(HttpStatusCode.Forbidden, ex.Message);
            }
            catch (ValidationException ex)
            {
                throw new ErrorException(HttpStatusCode.BadRequest, ex.Message, ProblemCode.ContentValidation);
            }
            catch (EPiServerException ex) when (ex.Message.Equals("You cannot save a content that is read-only. Call CreateWritableClone() on content and pass the cloned instance instead.", StringComparison.OrdinalIgnoreCase))
            {
                throw new ErrorException(HttpStatusCode.BadRequest, ex.Message, ProblemCode.ReadOnlyContent);
            }
            catch (EPiServerException ex) when (ex.Message.Contains("is not allowed to be created under parent"))
            {
                throw new ErrorException(HttpStatusCode.BadRequest, ex.Message, ProblemCode.NotAllowedParent);
            }
            catch (InvalidOperationException ex) when (ex.Message.Equals("StartPublish must be set when content item is set for scheduled publishing", StringComparison.OrdinalIgnoreCase))
            {
                throw new ErrorException(HttpStatusCode.BadRequest, ex.Message, ProblemCode.ScheduledPublishing);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("is not valid on this content"))
            {
                throw new ErrorException(HttpStatusCode.BadRequest, ex.Message, ProblemCode.InvalidAction);
            }
            catch (InvalidOperationException ex) when (ex.Message.EndsWith("does not implement ILocalizable"))
            {
                throw new ErrorException(HttpStatusCode.BadRequest, ex.Message, ProblemCode.ContentNotLocalizable);
            }
            catch (NotSupportedException ex)
            {
                throw new ErrorException(HttpStatusCode.BadRequest, ex.Message, ProblemCode.ContentProvider);
            }
            catch (ArgumentException ex) when (ex.Message.Equals("Cannot force new version and force current version at the same time.", StringComparison.OrdinalIgnoreCase))
            {
                throw new ErrorException(HttpStatusCode.BadRequest, ex.Message, ProblemCode.ForceVersion);
            }
            catch (ArgumentException ex) when (ex.Message.Equals("The delayed published flag must be used in combination with the check-in flag.", StringComparison.OrdinalIgnoreCase))
            {
                throw new ErrorException(HttpStatusCode.BadRequest, ex.Message, ProblemCode.DelayedPulish);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("is not valid on this content"))
            {
                throw new ErrorException(HttpStatusCode.BadRequest, ex.Message, ProblemCode.InvalidAction);
            }
            catch (SqlException ex) when (
                    ex.Message.StartsWith("The INSERT statement conflicted with the FOREIGN KEY constraint \"FK_tblWorkContentProperty_tblContent\"") ||
                    ex.Message.StartsWith("The UPDATE statement conflicted with the FOREIGN KEY constraint \"FK_tblWorkContentProperty_tblContent\""))
            {
                throw new ErrorException(HttpStatusCode.BadRequest, "The provided property content reference doesn't exist.", ProblemCode.PropertyReferenceNotFound);
            }
        }

        private SaveAction GetSaveAction(VersionStatus? versionStatus, ContentValidationMode validationMode)
        {
            var saveAction = SaveAction.Default;
            if (!versionStatus.HasValue)
            {
                return saveAction;
            }

            switch (versionStatus)
            {
                case VersionStatus.Rejected:
                    saveAction = SaveAction.Reject;
                    break;
                case VersionStatus.CheckedOut:
                    saveAction = SaveAction.CheckOut;
                    break;
                case VersionStatus.CheckedIn:
                    saveAction = SaveAction.CheckIn;
                    break;
                case VersionStatus.Published:
                    saveAction = SaveAction.Publish;
                    break;
                case VersionStatus.DelayedPublish:
                    saveAction = SaveAction.Schedule;
                    break;
                case VersionStatus.AwaitingApproval:
                    saveAction = SaveAction.RequestApproval;
                    break;
                default:
                    throw new ErrorException(HttpStatusCode.BadRequest, $"Cannot create a content with status {versionStatus}", ProblemCode.StatusTransition);
            }

            if (validationMode == ContentValidationMode.Minimal)
            {
                saveAction |= SaveAction.SkipValidation;
            }

            return saveAction;
        }

        private void AssignProperties(IContent content, IDictionary<string, object> inputProperties, bool shouldClearPropertyValueIfMissing)
        {
            AssignCategory(content, inputProperties, shouldClearPropertyValueIfMissing);

            foreach (var property in content.Property.Where(x => !x.IsMetaData))
            {
                if (inputProperties.ContainsKey(property.Name))
                {
                    var prop = inputProperties[property.Name];
                    var propertyModel = prop as IPropertyModel;
                    var converter = _propertyDataValueConverterResolver.Resolve(propertyModel);

                    if (converter is null)
                    {
                        throw new ErrorException(HttpStatusCode.BadRequest, $"Cannot handle property {property.Name}");
                    }

                    property.Value = converter.Convert(propertyModel, property);
                }
                else
                {
                    if (shouldClearPropertyValueIfMissing)
                    {
                        property.Clear();
                    }
                }
            }
        }

        private void AssignCategory(IContent content, IDictionary<string, object> inputProperties, bool shouldClearPropertyValueIfMissing)
        {
            if (content is ICategorizable categorizable)
            {
                if (inputProperties.TryGetValue(nameof(ICategorizable.Category), out var categoryProperty))
                {
                    categorizable.Category = new CategoryList();                    
                    foreach(var category in GetCategory(categoryProperty))
                    {
                        categorizable.Category.Add(category);
                    }
                    
                    // Remove the category in the property model list so it will not be handled by property converter later on.
                    inputProperties.Remove(nameof(ICategorizable.Category));
                }
                else
                {
                    if (shouldClearPropertyValueIfMissing && categorizable.Category is object)
                    {
                        categorizable.Category.Clear();
                    }                    
                }
            }
        }

        private void AssignVersionableContent(IContent content, ContentApiPatchModel patchModel)
        {
            if (content is IVersionable versionable)
            {
                if (patchModel.UpdatedMetadata.Contains(nameof(ContentApiPatchModel.StartPublish)))
                {
                    versionable.StartPublish = patchModel.StartPublish?.UtcDateTime;
                }

                if (patchModel.UpdatedMetadata.Contains(nameof(ContentApiPatchModel.StopPublish)))
                {
                    versionable.StopPublish = patchModel.StopPublish?.UtcDateTime;
                }
            }
            else if (patchModel.StartPublish.HasValue || patchModel.StopPublish.HasValue || patchModel.Status.HasValue)
            {
                throw new ErrorException(HttpStatusCode.BadRequest, "Cannot set (StartPublish or StopPublish or Status) for content that isn't versionable", ProblemCode.ContentNotVersionable);
            }
        }

        private void AssignVersionableContent(IContent content, ContentApiCreateModel inputContentModel)
        {
            if (content is IVersionable versionable)
            {
                if (!inputContentModel.Status.HasValue)
                {
                    throw new ErrorException(HttpStatusCode.BadRequest, "Missing status value on IVersionable content");
                }

                versionable.StartPublish = inputContentModel.StartPublish?.UtcDateTime;
                versionable.StopPublish = inputContentModel.StopPublish?.UtcDateTime;
            }
            else if (inputContentModel.StartPublish.HasValue || inputContentModel.StopPublish.HasValue || inputContentModel.Status.HasValue)
            {
                throw new ErrorException(HttpStatusCode.BadRequest, "Cannot set (StartPublish or StopPublish or Status) for content that isn't versionable", ProblemCode.ContentNotVersionable);
            }
        }

        private void AssignRoutableContent(IContent content, string routeSegment)
        {
            if (content is IRoutable routableContent)
            {
                routableContent.RouteSegment = routeSegment;
            }
            else if (!string.IsNullOrEmpty(routeSegment))
            {
                throw new ErrorException(HttpStatusCode.BadRequest, "Cannot set route segment for the content that isn't IRoutable", ProblemCode.ContentNotRoutable);
            }
        }

        private CategoryList GetCategory(object categoryProperty)
        {
            var categoryPropertyModel = categoryProperty as CategoryPropertyModel;
            if (categoryPropertyModel is null)
            {
                throw new ErrorException(HttpStatusCode.BadRequest, $"Invalid category");
            }

            var categoryConverter = _propertyDataValueConverterResolver.Resolve(categoryPropertyModel);
            if (categoryConverter is null)
            {
                throw new ErrorException(HttpStatusCode.BadRequest, $"Cannot handle property {nameof(ICategorizable.Category)}");
            }

            return (CategoryList)categoryConverter.Convert(categoryPropertyModel, null);
        }

        private bool TryGetContent(ContentReferenceInputModel contentReference, out IContent content)
        {
            content = null;

            if (contentReference is null)
            {
                return false;
            }

            if (!contentReference.Id.HasValue && !contentReference.GuidValue.HasValue)
            {
                return false;
            }

            return contentReference.Id.HasValue ? _contentRepository.TryGet(new ContentReference(contentReference.Id.Value), out content)
                    : _contentRepository.TryGet(contentReference.GuidValue.Value, out content);
        }

        private void EvaluateRequiredRole(IContent content)
        {
            if (!_requiredRoleEvaluator.HasAccess(content))
            {
                throw new ErrorException(HttpStatusCode.Forbidden, $"The content {content.ContentGuid} can not be accessed by Content Management API.");
            }
        }

        private bool IsSystemContent(ContentReference contentLink)
        {
            if (ContentReference.IsNullOrEmpty(contentLink))
            {
                return false;
            }

            var siteDefinitions = _siteDefinitionRepository.List();
            return siteDefinitions.Any(site => site.StartPage.CompareToIgnoreWorkID(contentLink) ||
                                               site.WasteBasket.CompareToIgnoreWorkID(contentLink) ||
                                               site.RootPage.CompareToIgnoreWorkID(contentLink) ||
                                               site.GlobalAssetsRoot.CompareToIgnoreWorkID(contentLink) ||
                                               site.SiteAssetsRoot.CompareToIgnoreWorkID(contentLink) ||
                                               site.ContentAssetsRoot.CompareToIgnoreWorkID(contentLink));
        }

        private bool Delete(IContent content, bool permanentDelete)
        {
            if (content == null)
            {
                return false;
            }

            EvaluateRequiredRole(content);

            try
            {
                if (IsSystemContent(content.ContentLink))
                {
                    throw new ErrorException(HttpStatusCode.BadRequest, "Cannot delete system content.", ProblemCode.SystemContent);
                }

                if (permanentDelete)
                {
                    _contentRepository.Delete(content.ContentLink, true);
                    return true;
                }

                if (content.IsDeleted)
                {
                    throw new ErrorException(HttpStatusCode.Conflict, "The content item is already in the recycle bin. Deleting it would meant that it would be permanently deleted.");
                }

                _contentRepository.MoveToWastebasket(content.ContentLink);
                return true;
            }
            catch (AccessDeniedException ex)
            {
                throw new ErrorException(HttpStatusCode.Forbidden, ex.Message);
            }
        }

        private bool MoveInternal(IContent sourceContent, ContentReferenceInputModel parentLink)
        {
            if (sourceContent is null)
            {
                throw new ArgumentNullException(nameof(sourceContent));
            }

            if (parentLink is null)
            {
                throw new ArgumentNullException(nameof(parentLink));
            }

            if (!TryGetContent(parentLink, out var destination))
            {
                throw new ErrorException(HttpStatusCode.BadRequest, $"Cannot get parent content", ProblemCode.InvalidParent);
            }

            EvaluateRequiredRole(sourceContent);
            EvaluateRequiredRole(destination);

            if (IsSystemContent(sourceContent.ContentLink))
            {
                throw new ErrorException(HttpStatusCode.BadRequest, "Cannot move system content.", ProblemCode.SystemContent);
            }

            try
            {
                _contentRepository.Move(sourceContent.ContentLink, destination.ContentLink);
                return true;
            }
            catch (AccessDeniedException ex)
            {
                throw new ErrorException(HttpStatusCode.Forbidden, ex.Message);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is ValidationException || ex is NotSupportedException || ex is EPiServerCancelException)
            {
                throw new ErrorException(HttpStatusCode.BadRequest, ex.Message);
            }
        }
    }
}
