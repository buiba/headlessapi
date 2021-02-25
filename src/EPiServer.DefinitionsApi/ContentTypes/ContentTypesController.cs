using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using EPiServer.ContentApi.Cors.Internal;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.ContentApi.Security.Internal;
using EPiServer.DataAbstraction;
using EPiServer.DefinitionsApi.ContentTypes.Internal;
using EPiServer.DefinitionsApi.Internal;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;

namespace EPiServer.DefinitionsApi.ContentTypes
{
    /// <summary>
    /// REST Controller for content types.
    /// </summary>
    [Error]
    [Cors]
    [ApiAuthorize]
    public class ContentTypesController : ApiController
    {
        internal const string RoutePrefix = ApiRoute.Prefix + "contenttypes/";
        private const string GetContentTypeRouteName = "GetContentType";
        private readonly ExternalContentTypeRepository _repository;
        private readonly ContentTypeAnalyzer _contentTypeAnalyzer;

        // Default constructor is required as we don't want to require that applications implement
        // a DependencyResolver

        /// <exclude />
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1604:Element documentation should have summary", Justification = "Member is excluded")]
        public ContentTypesController()
            : this(
                ServiceLocator.Current.GetInstance<ExternalContentTypeRepository>(),
                ServiceLocator.Current.GetInstance<ContentTypeAnalyzer>())
        { }

        internal ContentTypesController(ExternalContentTypeRepository repository, ContentTypeAnalyzer contentTypeAnalyzer)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _contentTypeAnalyzer = contentTypeAnalyzer ?? throw new ArgumentNullException(nameof(contentTypeAnalyzer));
        }

        /// <summary>
        /// List all content types in the system.
        /// </summary>
        /// <param name="top">The maximum number of returned content types.</param>
        /// <param name="continuationToken">A token identifying a position to continue from a previously paged response.</param>
        /// <returns>A list of all content types.</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad request</response>
        [Route(RoutePrefix)]
        [HttpGet]
        [ResponseType(typeof(ExternalContentType[]))]
        public IHttpActionResult List(
            int? top = null,
            [FromContinuationTokenHeader] string continuationToken = default)
        {
            if (!ContinuationToken.TryParseTokenString(continuationToken, out var token))
            {
                return this.Problem(HttpStatusCode.BadRequest, "Unable to recognize the provided continuation token.");
            }

            ListResult listResult;
            if (token == ContinuationToken.None)
            {
                listResult = _repository.List(top);
            }
            else if (top is null)
            {
                listResult = _repository.List(token);
            }
            else
            {
                return this.Problem(HttpStatusCode.BadRequest, "Only one of the parameters 'top' and 'continuationToken' are allowed simultaneously.");
            }

            return new ContentTypeListResult(listResult, this);
        }

        /// <summary>
        /// Gets the content type at the current location.
        /// </summary>
        /// <param name="id">The identifier of the content type.</param>
        /// <returns>The content type.</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad request</response>
        /// <response code="404">Not found</response>
        [Route(RoutePrefix + "{id}", Name = GetContentTypeRouteName)]
        [HttpGet]
        [ResponseType(typeof(ExternalContentType))]
        public IHttpActionResult Get(Guid id)
        {
            var contentType = _repository.Get(id);

            if (contentType is null)
            {
                return NotFound();
            }

            return Json(contentType);
        }

        /// <summary>
        /// Anayze content types.
        /// </summary>
        /// <param name="contentType">The content type that should be analyzed.</param>
        /// <returns>List of content type differences.</returns>
        /// <response code="200">Ok</response>
        [Route(RoutePrefix + "analyze")]
        [HttpPost]
        [ResponseType(typeof(ExternalContentTypeDifference[]))]
        public IHttpActionResult Analyze([Required][FromBody] ExternalContentType contentType)
        {
            if (contentType is null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            return Json(_contentTypeAnalyzer.Analyze(contentType));
        }

        /// <summary>
        /// Creates a new content type in the system.
        /// </summary>
        /// <param name="contentType">The content type that should be created.</param>
        /// <response code="201">Created</response>
        /// <response code="400">Bad request</response>
        /// <response code="409">Conflict</response>
        [Route(RoutePrefix)]
        [HttpPost]
        [ValidateModel]
        [ResponseType(typeof(ExternalContentType))]
        public IHttpActionResult Create([Required][FromBody] ValidatableExternalContentType contentType)
        {
            if (contentType is null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            if (contentType.Id == Guid.Empty)
            {
                var existing = _repository.Get(contentType.Name);
                if (existing is object)
                {
                    return this.Problem(HttpStatusCode.Conflict, "There is already another content type with the provided name.");
                }

                contentType.Id = Guid.NewGuid();
            }
            else
            {
                var existing = _repository.Get(contentType.Id);
                if (existing is object)
                {
                    return this.Problem(HttpStatusCode.Conflict, "There is already another content type with the provided id.");
                }
            }

            _repository.Save(new[] { contentType }, out var createdContentTypes, VersionComponent.None, VersionComponent.Major);

            return CreatedAtRoute(GetContentTypeRouteName, new { id = createdContentTypes.First() }, contentType);
        }

        /// <summary>
        /// Updates the content type at the current location or create a new one if it doesn't already exist.
        /// </summary>
        /// <param name="id">The identifier of the content type.</param>
        /// <param name="contentType">The content type that should be created or updated.</param>
        /// <param name="allowedDowngrades">Defines which types of downgrades that are allowed.</param>
        /// <param name="allowedUpgrades">Defines which types of upgrades that are allowed.</param>
        /// <response code="200">Ok</response>
        /// <response code="201">Created</response>
        /// <response code="400">Bad request</response>
        /// <response code="409">Conflict</response>
        [Route(RoutePrefix + "{id}")]
        [HttpPut]
        [ValidateModel]
        [ResponseType(typeof(ExternalContentType))]
        public IHttpActionResult CreateOrUpdate(Guid id, [Required][FromBody] ValidatableExternalContentType contentType, VersionComponent? allowedDowngrades = null, VersionComponent? allowedUpgrades = null)
        {
            if (contentType is null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            if (contentType.Id == Guid.Empty)
            {
                contentType.Id = id;
            }
            else if (contentType.Id != id)
            {
                return this.Problem(HttpStatusCode.BadRequest, "The id on the provided content type does not match the resource location and cannot be changed.");
            }

            _repository.Save(new[] { contentType }, out var createdContentTypes, allowedDowngrades, allowedUpgrades);

            if (createdContentTypes.Any())
            {
                return CreatedAtRoute(GetContentTypeRouteName, new { id = createdContentTypes.First() }, contentType);
            }

            return Json(contentType);
        }

        /// <summary>
        /// Deletes the content type at this location.
        /// </summary>
        /// <param name="id">The identifier of the content type.</param>
        /// <response code="204">No content</response>
        /// <response code="404">Ok</response>
        /// <response code="409">Conflict</response>
        [Route(RoutePrefix + "{id}")]
        [HttpDelete]
        [ResponseType(typeof(void))]
        public IHttpActionResult Delete(Guid id)
        {
            if (_repository.TryDelete(id))
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            return NotFound();
        }
    }
}
