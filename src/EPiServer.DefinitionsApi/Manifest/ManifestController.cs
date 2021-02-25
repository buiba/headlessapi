using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using EPiServer.ContentApi.Cors.Internal;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.ContentApi.Security.Internal;
using EPiServer.DefinitionsApi.Internal;
using EPiServer.DefinitionsApi.Manifest.Internal;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;

namespace EPiServer.DefinitionsApi.Manifest
{
    /// <summary>
    /// REST Controller for importing application manifest.
    /// </summary>
    [Error]
    [Cors]
    [ApiAuthorize]
    public class ManifestController : ApiController
    {
        internal const string RoutePrefix = ApiRoute.Prefix + "manifest/";
        private readonly ManifestService _manifestService;

        /// <exclude />
        /// <summary>
        /// Default constructor is required as we don't want to require that applications implement a DependencyResolver
        /// </summary>
        public ManifestController()
            : this(ServiceLocator.Current.GetInstance<ManifestService>())
        { }

        internal ManifestController(ManifestService manifestService)
        {
            _manifestService = manifestService ?? throw new ArgumentNullException(nameof(manifestService));
        }

        /// <summary>
        /// Imports a manifest.
        /// </summary>
        /// <param name="model">The manifest to import.</param>
        /// <param name="continueOnError">Indicates whether next manifest section importer should continue when the previous fails.</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad request</response>
        [Route(RoutePrefix)]
        [ValidateModel]
        [HttpPut]
        [ResponseType(typeof(ImportLogMessage[]))]
        public IHttpActionResult Put([Required][FromBody] ManifestModel model, [FromUri] bool continueOnError = true)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var context = new ImportContext
            {
                ContinueOnError = continueOnError
            };

            try
            {
                _manifestService.ImportManifestSections(model.Sections.Values, context);
            }
            catch (Exception ex)
            {
                return this.Problem(HttpStatusCode.BadRequest, ex.Message);
            }

            return Json(context.Log);
        }
    }
}
