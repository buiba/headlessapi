﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ValueProviders;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.ContentResult;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.Cors.Internal;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.ContentApi.Security.Internal;
using EPiServer.ContentManagementApi.Internal;
using EPiServer.ContentManagementApi.Models.Internal;
using EPiServer.ContentManagementApi.Serialization.Internal;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace EPiServer.ContentManagementApi.Controllers
{
    /// <summary>
    /// REST Controller for content management.
    /// </summary>
    [Error]
    [Cors]
    [ApiAuthorize]
    public class ContentManagementApiController : ApiController
    {
        internal const string RoutePrefix = ApiRoute.Prefix + "contentmanagement/";
        private const string GetContent = "GetContent";
        private const string GetContentById = "GetContentById";

        private readonly ContentManagementRepository _contentManagementRepository;
        private readonly ContentApiSerializerResolver _contentSerializerResolver;
        private readonly ContentConvertingService _contentConvertingService;
        private readonly ContentApiConfiguration _apiConfiguration;
        private readonly ISecurityPrincipal _principalAccessor;
        private readonly IContentLoader _contentLoader;
        private readonly UserService _userService;
        private readonly ContentApiPropertyModelConverter _contentApiPropertyModelConverter;
        private readonly JsonSerializer _jsonSerializer = new JsonSerializer()
        {
            NullValueHandling = GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.NullValueHandling
        };

        public ContentManagementApiController() :
            this(ServiceLocator.Current.GetInstance<ContentManagementRepository>(),
                ServiceLocator.Current.GetInstance<ContentApiSerializerResolver>(),
                ServiceLocator.Current.GetInstance<ContentConvertingService>(),
                ServiceLocator.Current.GetInstance<ContentApiConfiguration>(),
                ServiceLocator.Current.GetInstance<IContentLoader>(),
                ServiceLocator.Current.GetInstance<ISecurityPrincipal>(),
                ServiceLocator.Current.GetInstance<UserService>(),
                ServiceLocator.Current.GetInstance<ContentApiPropertyModelConverter>())
        {

        }

        internal ContentManagementApiController(ContentManagementRepository contentManagementRepository,
            ContentApiSerializerResolver contentSerializerResolver,
            ContentConvertingService contentConvertingService,
            ContentApiConfiguration apiConfiguration,
            IContentLoader contentLoader,
            ISecurityPrincipal principalAccessor,
            UserService userService,
            ContentApiPropertyModelConverter contentApiPropertyModelConverter)
        {
            _contentManagementRepository = contentManagementRepository ?? throw new ArgumentNullException(nameof(contentManagementRepository));
            _contentSerializerResolver = contentSerializerResolver ?? throw new ArgumentNullException(nameof(contentSerializerResolver));
            _contentConvertingService = contentConvertingService ?? throw new ArgumentNullException(nameof(contentConvertingService));
            _apiConfiguration = apiConfiguration;
            _contentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
            _principalAccessor = principalAccessor ?? throw new ArgumentNullException(nameof(principalAccessor));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _contentApiPropertyModelConverter = contentApiPropertyModelConverter ?? throw new ArgumentNullException(nameof(contentApiPropertyModelConverter));
        }

        /// <summary>
        /// Delete a content by unique identifier.
        /// </summary>
        /// <param name="contentGuid">Unique identifier of the content to be deleted.</param>
        /// <param name="permanentDelete">Set to true in order to permanently delete the content. Otherwise it will be moved to the wastebasket. Read from the 'x-epi-permanent-delete' header.</param>
        /// <response code="204">No content</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        /// <response code="409">Conflict</response>
        [Route(RoutePrefix + "{contentGuid:guid}")]
        [HttpDelete]
        [HttpOptions]
        [ResponseType(typeof(void))]
        public IHttpActionResult DeleteByContentGuid(Guid contentGuid,
            [FromHeader(Name = HeaderConstants.PermanentDeleteHeaderName)] bool permanentDelete = false)
        {
            if (_contentManagementRepository.TryDelete(contentGuid, permanentDelete))
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            return NotFound();
        }

        /// <summary>
        /// Delete a content by content reference.
        /// </summary>
        /// <param name="contentReference">Content reference of the content to be deleted.</param>
        /// <param name="permanentDelete">Set to true in order to permanently delete the content. Otherwise it will be moved to the wastebasket. Read from the 'x-epi-permanent-delete' header.</param>
        /// <response code="204">No content</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        /// <response code="409">Conflict</response>
        [Route(RoutePrefix + "{contentReference}")]
        [HttpDelete]
        [HttpOptions]
        [ResponseType(typeof(void))]
        public IHttpActionResult DeleteByContentReference(string contentReference,
            [FromHeader(Name = HeaderConstants.PermanentDeleteHeaderName)] bool permanentDelete = false)
        {
            if (!ContentReference.TryParse(contentReference, out var reference))
            {
                return BadRequest($"'{contentReference}' is incorrect format.");
            }

            if (_contentManagementRepository.TryDelete(reference, permanentDelete))
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            return NotFound();
        }

        /// <summary>
        /// Get a common draft of a content by given unique identifier and language.
        /// </summary>
        /// <param name="contentGuid">Unique identifier of the content to be retrieved.</param>
        /// <param name="languages">Language of the content to be retrieved. Example: 'en' or 'sv'</param>
        /// <response code="200">Ok</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        [Route(RoutePrefix + "{contentGuid:guid}", Name = GetContent)]
        [HttpGet]
        [ResponseType(typeof(ContentApiModel))]
        public IHttpActionResult GetCommonDraft(Guid contentGuid, [ValueProvider(typeof(AcceptLanguageHeaderValueProviderFactory))] List<string> languages)
        {
            var language = languages?.FirstOrDefault();
            var content = _contentManagementRepository.GetCommonDraft(contentGuid, language);

            return content is null ? NotFound() : ResultFromContent(content, HttpStatusCode.OK, null);
        }

        /// <summary>
        /// Get a common draft of a content by given content reference and language.
        /// </summary>
        /// <param name="contentReference">Content reference of the content to be retrieved.</param>
        /// <param name="languages">Language of the content to be retrieved. Example: 'en' or 'sv'</param>
        /// <response code="200">Ok</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        [Route(RoutePrefix + "{contentReference}", Name = GetContentById)]
        [HttpGet]
        [ResponseType(typeof(ContentApiModel))]
        public IHttpActionResult Get(string contentReference,
            [ValueProvider(typeof(AcceptLanguageHeaderValueProviderFactory))] List<string> languages)
        {
            var language = languages?.FirstOrDefault();
            var content = _contentManagementRepository.GetCommonDraft(new ContentReference(contentReference), language);

            return content is null ? NotFound() : ResultFromContent(content, HttpStatusCode.OK, null);
        }

        /// <summary>
        /// Move a content from its current location to another location.
        /// </summary>
        /// <param name="contentGuid">Unique identifier of the content to be moved.</param>
        /// <param name="moveContentModel">Where the content will be moved to.</param>
        /// <response code="204">No content</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        [Route(RoutePrefix + "{contentGuid:guid}/move")]
        [HttpPost]
        [HttpOptions]
        [ValidateModel]
        [ResponseType(typeof(void))]
        public IHttpActionResult MoveByContentGuid(Guid contentGuid, [Required][FromBody] MoveContentModel moveContentModel)
        {
            if (moveContentModel is null)
            {
                return this.Problem(HttpStatusCode.BadRequest, "Request body is required.");
            }

            if (_contentManagementRepository.Move(contentGuid, moveContentModel.ParentLink))
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            return NotFound();
        }

        /// <summary>
        /// Move a content from its current location to another location.
        /// </summary>
        /// <param name="contentReference">Content reference of the content to be moved.</param>
        /// <param name="moveContentModel">Where the content will be moved to.</param>
        /// <response code="204">No content</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        [Route(RoutePrefix + "{contentReference}/move")]
        [HttpPost]
        [HttpOptions]
        [ValidateModel]
        [ResponseType(typeof(void))]
        public IHttpActionResult MoveByContentReference(string contentReference, [Required][FromBody] MoveContentModel moveContentModel)
        {
            if (moveContentModel is null)
            {
                return this.Problem(HttpStatusCode.BadRequest, "Request body is required.");
            }

            if (!ContentReference.TryParse(contentReference, out var reference))
            {
                return BadRequest($"'{contentReference}' is incorrect format.");
            }

            if (_contentManagementRepository.Move(reference, moveContentModel.ParentLink))
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            return NotFound();
        }

        /// <summary>
        /// Create a new content item.
        /// </summary>
        /// <param name="contentApiCreateModel">Contains information of the new content to be created.</param>
        /// <param name="validationMode">The validation mode used when saving content.</param>
        /// <response code="201">Created</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="409">Conflict</response>
        [Route(RoutePrefix)]
        [HttpPost]
        [HttpOptions]
        [ValidateModel]
        [ResponseType(typeof(ContentApiModel))]
        public IHttpActionResult Create(
            [Required][FromBody] ContentApiCreateModel contentApiCreateModel,
            [FromHeader(Name = HeaderConstants.ValidationMode)] ContentValidationMode validationMode = ContentValidationMode.Complete)
        {
            if (contentApiCreateModel is null)
            {
                return this.Problem(HttpStatusCode.BadRequest, "Request body is required.");
            }

            var language = contentApiCreateModel.Language is object ? CultureInfo.GetCultureInfo(contentApiCreateModel.Language.Name) : null;

            var contentGuid = contentApiCreateModel.ContentLink?.GuidValue;
            if (_contentLoader.TryGet<IContent>(contentGuid.GetValueOrDefault(), out var _))
            {
                return this.Problem(HttpStatusCode.Conflict, $"The provided content link {contentGuid.GetValueOrDefault()} is already existed.");
            }

            contentApiCreateModel.Properties = _contentApiPropertyModelConverter.ConvertRawPropertiesToPropertyModels(contentApiCreateModel.ContentType, contentApiCreateModel.Properties, _jsonSerializer);

            var contentReference = _contentManagementRepository.Create(contentApiCreateModel, new SaveContentOptions(validationMode));
            var content = _contentLoader.Get<IContent>(contentReference, language);
            var headers = new Dictionary<string, string>
            {
                { HeaderConstants.Location, Url.Link(GetContent, new { contentGuid = content.ContentGuid }) }
            };

            return ResultFromContent(content, HttpStatusCode.Created, headers);
        }

        /// <summary>
        /// Create a new version of a content by a given content reference.
        /// </summary>
        /// <param name="contentReference">Content reference of a content that will create the new content version from.</param>
        /// <param name="contentApiCreateModel">Contains information of the new version to be created.</param>
        /// <param name="validationMode">The validation mode used when saving content.</param>
        /// <response code="201">Created</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        [Route(RoutePrefix + "{contentReference}")]
        [HttpPost]
        [ValidateModel]
        [ResponseType(typeof(ContentApiModel))]
        public IHttpActionResult CreateVersionByContentReference(
            string contentReference,
            [Required][FromBody] ContentApiCreateModel contentApiCreateModel,
            [FromHeader(Name = HeaderConstants.ValidationMode)] ContentValidationMode validationMode = ContentValidationMode.Complete)
        {
            if (!ContentReference.TryParse(contentReference, out var contentLink))
            {
                return BadRequest($"'{contentReference}' is incorrect format.");
            }

            if (contentLink.WorkID > 0)
            {
                return this.Problem(HttpStatusCode.BadRequest, $"Provide a ContentReference with a WorkID as {contentReference} is invalid.");
            }

            if (contentApiCreateModel is null)
            {
                return this.Problem(HttpStatusCode.BadRequest, "Request body is required.");
            }

            if (contentApiCreateModel.ContentLink?.Id is object && contentApiCreateModel.ContentLink.Id != contentLink.ID)
            {
                return this.Problem(HttpStatusCode.BadRequest, $"The content reference {contentApiCreateModel.ContentLink.Id} on the provided content does not match the resource location.");
            }

            contentApiCreateModel.Properties = _contentApiPropertyModelConverter.ConvertRawPropertiesToPropertyModels(contentReference,
                contentApiCreateModel.Properties, _jsonSerializer);

            var newVersionReference = _contentManagementRepository.CreateVersion(contentLink, contentApiCreateModel, new SaveContentOptions(validationMode));

            var language = contentApiCreateModel.Language is object ? CultureInfo.GetCultureInfo(contentApiCreateModel.Language.Name) : null;
            var content = _contentLoader.Get<IContent>(newVersionReference, language);
            var headers = new Dictionary<string, string>
            {
                { HeaderConstants.Location, Url.Link(GetContentById, new { contentReference = contentLink.ID }) }
            };

            return ResultFromContent(content, HttpStatusCode.Created, headers);
        }

        /// <summary>
        /// Create a new version of a content by a given unique identifier.
        /// </summary>
        /// <param name="contentGuid">Unique identifier of a content that will create the new content version from.</param>
        /// <param name="contentApiCreateModel">Contains information of the new version to be created.</param>
        /// <param name="validationMode">The validation mode used when saving content.</param>
        /// <response code="201">Created</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        [Route(RoutePrefix + "{contentGuid:guid}")]
        [HttpPost]
        [ValidateModel]
        [ResponseType(typeof(ContentApiModel))]
        public IHttpActionResult CreateVersionByContentGuid(
            Guid contentGuid,
            [Required][FromBody] ContentApiCreateModel contentApiCreateModel,
            [FromHeader(Name = HeaderConstants.ValidationMode)] ContentValidationMode validationMode = ContentValidationMode.Complete)
        {
            if (contentApiCreateModel is null)
            {
                return this.Problem(HttpStatusCode.BadRequest, "Request body is required.");
            }

            if (contentApiCreateModel.ContentLink?.GuidValue is object && contentApiCreateModel.ContentLink.GuidValue != contentGuid)
            {
                return this.Problem(HttpStatusCode.BadRequest, $"The guid {contentApiCreateModel.ContentLink.GuidValue} on the provided content does not match the resource location.");
            }

            contentApiCreateModel.Properties = _contentApiPropertyModelConverter.ConvertRawPropertiesToPropertyModels(contentGuid.ToString(),
                contentApiCreateModel.Properties, _jsonSerializer);

            var contentReference = _contentManagementRepository.CreateVersion(contentGuid, contentApiCreateModel, new SaveContentOptions(validationMode));

            var language = contentApiCreateModel.Language is object ? CultureInfo.GetCultureInfo(contentApiCreateModel.Language.Name) : null;
            var createdContent = _contentLoader.Get<IContent>(contentReference, language);
            var headers = new Dictionary<string, string>
            {
                { HeaderConstants.Location, Url.Link(GetContent, new { contentGuid }) }
            };

            return ResultFromContent(createdContent, HttpStatusCode.Created, headers);
        }

        /// <summary>
        /// Update the specified content item.
        /// </summary>
        /// <param name="contentReference">Which content item to update.</param>
        /// <param name="contentApiPatchModel">How the content item should be updated.</param>
        /// <param name="validationMode">The validation mode used when saving. Optional, defaults to <see cref="ContentValidationMode.Complete" />.</param>
        /// <response code="204">No Content</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        [Route(RoutePrefix + "{contentReference}")]
        [HttpPatch]
        [ValidateModel]
        [ResponseType(typeof(void))]
        public IHttpActionResult PatchByContentReference(string contentReference, [Required][FromBody] ContentApiPatchModel contentApiPatchModel,
            [FromHeader(Name = HeaderConstants.ValidationMode)] ContentValidationMode validationMode = ContentValidationMode.Complete)
        {
            if (!ContentReference.TryParse(contentReference, out var contentLink))
            {
                return BadRequest($"'{contentReference}' is incorrect format.");
            }

            if (contentLink.WorkID > 0)
            {
                return this.Problem(HttpStatusCode.BadRequest, $"Provide a ContentReference with a WorkID as {contentReference} is invalid.");
            }

            if (contentApiPatchModel is null)
            {
                return this.Problem(HttpStatusCode.BadRequest, "Request body is required.");
            }

            contentApiPatchModel.Properties = _contentApiPropertyModelConverter.ConvertRawPropertiesToPropertyModels(contentReference,
                contentApiPatchModel.Properties, _jsonSerializer);

            _ = _contentManagementRepository.Patch(contentLink, contentApiPatchModel, new SaveContentOptions(validationMode));

            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Update the specified content item.
        /// </summary>
        /// <param name="contentGuid">Which content item to update.</param>
        /// <param name="contentApiPatchModel">How the content item should be updated.</param>
        /// <param name="validationMode">The validation mode used when saving. Optional, defaults to <see cref="ContentValidationMode.Complete" />.</param>
        /// <response code="204">No Content</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        [Route(RoutePrefix + "{contentGuid:guid}")]
        [HttpPatch]
        [ValidateModel]
        [ResponseType(typeof(void))]
        public IHttpActionResult PatchByContentGuid(Guid contentGuid, [Required][FromBody] ContentApiPatchModel contentApiPatchModel,
            [FromHeader(Name = HeaderConstants.ValidationMode)] ContentValidationMode validationMode = ContentValidationMode.Complete)
        {
            if (contentApiPatchModel is null)
            {
                return this.Problem(HttpStatusCode.BadRequest, "Request body is required.");
            }

            contentApiPatchModel.Properties = _contentApiPropertyModelConverter.ConvertRawPropertiesToPropertyModels(contentGuid.ToString(),
                contentApiPatchModel.Properties, _jsonSerializer);

            _ = _contentManagementRepository.Patch(contentGuid, contentApiPatchModel, new SaveContentOptions(validationMode));

            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        ///  Updates the content item at the current location or create a new one if it doesn't exist.
        /// </summary>
        /// <param name="contentGuid">Guid of the content that should be created/updated.</param>
        /// <param name="contentApiCreateModel">The model that contains information for creating/updating content item.</param>
        /// <param name="validationMode">The validation mode used when saving content.</param>
        /// <response code="200">Ok</response>
        /// <response code="201">Created</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">NotFound</response>
        [Route(RoutePrefix + "{contentGuid:guid}")]
        [HttpPut]
        [ValidateModel]
        [ResponseType(typeof(ContentApiModel))]
        public IHttpActionResult CreateOrUpdate(Guid contentGuid,
            [Required][FromBody] ContentApiCreateModel contentApiCreateModel,
            [FromHeader(Name = HeaderConstants.ValidationMode)] ContentValidationMode validationMode = ContentValidationMode.Complete)
        {
            if (contentApiCreateModel is null)
            {
                return this.Problem(HttpStatusCode.BadRequest, "Request body is required.");
            }

            if (contentApiCreateModel.ContentLink is null)
            {
                contentApiCreateModel.ContentLink = new ContentReferenceInputModel();
            }

            if (!contentApiCreateModel.ContentLink.GuidValue.HasValue)
            {
                contentApiCreateModel.ContentLink.GuidValue = contentGuid;
            }
            else if (contentApiCreateModel.ContentLink.GuidValue.Value != contentGuid)
            {
                return this.Problem(HttpStatusCode.BadRequest, $"The guid value '{contentApiCreateModel.ContentLink.GuidValue}' on the provided content does not match the resource location and cannot be changed.");
            }

            if (_contentLoader.TryGet<IContent>(contentGuid, out var _))
            {
                contentApiCreateModel.Properties = _contentApiPropertyModelConverter.ConvertRawPropertiesToPropertyModels(contentGuid.ToString(), contentApiCreateModel.Properties, _jsonSerializer);

                _contentManagementRepository.Update(contentGuid, contentApiCreateModel, new SaveContentOptions(validationMode));

                var language = contentApiCreateModel.Language is object ? CultureInfo.GetCultureInfo(contentApiCreateModel.Language.Name) : null;
                var createdContent = _contentManagementRepository.GetCommonDraft(contentGuid, language?.Name);

                return ResultFromContent(createdContent, HttpStatusCode.OK, null);
            }
            else
            {
                contentApiCreateModel.Properties = _contentApiPropertyModelConverter.ConvertRawPropertiesToPropertyModels(contentApiCreateModel.ContentType, contentApiCreateModel.Properties, _jsonSerializer);

                var contentReference = _contentManagementRepository.Create(contentApiCreateModel, new SaveContentOptions(validationMode));

                var language = contentApiCreateModel.Language is object ? CultureInfo.GetCultureInfo(contentApiCreateModel.Language.Name) : null;
                var createdContent = _contentLoader.Get<IContent>(contentReference, language);

                var headers = new Dictionary<string, string>
                {
                    { HeaderConstants.Location, Url.Link(GetContent, new { contentGuid }) }
                };

                return ResultFromContent(createdContent, HttpStatusCode.Created, headers);
            }
        }

        private IHttpActionResult ResultFromContent(IContent content, HttpStatusCode statusCode, IDictionary<string, string> headers)
        {
            if (!_userService.IsUserAllowedToAccessContent(content, _principalAccessor.GetCurrentPrincipal(), AccessLevel.Read))
            {
                throw new ErrorException(HttpStatusCode.Forbidden, ErrorCode.Forbidden);
            }

            var model = _contentConvertingService.Convert(content, CreateContext());
            return new ContentApiResult<IContentApiModel>(model, statusCode, headers, _apiConfiguration.Default(), _contentSerializerResolver);
        }

        private ConverterContext CreateContext() => new ConverterContext(_apiConfiguration.Default(), string.Empty,
            string.Empty, false, null, ContextMode.Edit, true);
    }
}
