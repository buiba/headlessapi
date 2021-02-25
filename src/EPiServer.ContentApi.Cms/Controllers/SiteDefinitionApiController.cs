using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using EPiServer.ContentApi.Cms.Internal;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.ContentResult;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.Core.OutputCache;
using EPiServer.ContentApi.Core.OutputCache.Internal;
using EPiServer.ContentApi.Core.Security.Internal;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.Core.Tracking;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPiServer.ContentApi.Cms.Controllers
{
    /// <summary>
    /// Controller for returning requests for SiteDefinition with appropriate filtering
    /// </summary>
    [ContentApiAuthorization]
    [ContentApiCors]
    [CorsOptionsActionFilter]
    [OutputCacheFilter(new[] { DependencyTypes.Site })]
    [SiteDefinitionApiFilter]
    public partial class SiteDefinitionApiController : ApiController
    {
        private const string VersionTwoSiteBase = RouteConstants.VersionTwoApiRoute + "site/";
        private const string VersionThreeSiteBase = RouteConstants.VersionThreeApiRoute + "site/";
        private static readonly ILogger _log = LogManager.GetLogger(typeof(SiteDefinitionApiController));
        private readonly ISiteDefinitionRepository _siteDefinitionRepository;
        private readonly ServiceAccessor<SiteDefinition> _currentSiteAccessor;
        private readonly ContentApiConfiguration _apiConfiguration;
        private readonly ISiteDefinitionConverter _siteDefinitionConverter;
        private readonly IContentApiTrackingContextAccessor _contentApiTrackingContext;
        private readonly ContentApiSerializerResolver _contentApiSerializerResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteDefinitionApiController"/> class.
        /// </summary>
        public SiteDefinitionApiController()
            : this(ServiceLocator.Current.GetInstance<ISiteDefinitionRepository>(),
                   ServiceLocator.Current.GetInstance<ServiceAccessor<SiteDefinition>>(),
                   ServiceLocator.Current.GetInstance<ContentApiConfiguration>(),
                   ServiceLocator.Current.GetInstance<ISiteDefinitionConverter>(),
                   ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>(),
                   ServiceLocator.Current.GetInstance<ContentApiSerializerResolver>())
        {
        }        

        internal SiteDefinitionApiController(
            ISiteDefinitionRepository siteDefinitionRepository,
            ServiceAccessor<SiteDefinition> currentSiteAccessor,
            ContentApiConfiguration apiConfiguration,
            ISiteDefinitionConverter siteDefinitionConverter,
            IContentApiTrackingContextAccessor contentApiTrackingContext,
            ContentApiSerializerResolver contentApiSerializerResolver)
        {
            _siteDefinitionRepository = siteDefinitionRepository;
            _currentSiteAccessor = currentSiteAccessor;
            _apiConfiguration = apiConfiguration;
            _siteDefinitionConverter = siteDefinitionConverter;
            _contentApiTrackingContext = contentApiTrackingContext;
            _contentApiSerializerResolver = contentApiSerializerResolver;
        }

        /// <summary>
        /// Get SiteDefinitionModel of current request
        /// </summary>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        [Route(VersionThreeSiteBase)]
        [Route(VersionTwoSiteBase)]
        [HttpGet]
        [HttpOptions]
        [ResponseType(typeof(SiteDefinitionModel[]))]
        public IHttpActionResult Get()
        {
            var options = _apiConfiguration.Default();

            try
            {
                var context = CreateContext(options);

                if (options.MultiSiteFilteringEnabled)
                {
                    _contentApiTrackingContext.Current.ReferencedSites.Add(new ReferencedSiteMetadata(_currentSiteAccessor().Id, _currentSiteAccessor().Saved));
                    return new ContentApiResult<IEnumerable<SiteDefinitionModel>>(new[] { _siteDefinitionConverter.Convert(_currentSiteAccessor(), context) }, HttpStatusCode.OK);
                }

                var siteList = _siteDefinitionRepository.List();
                foreach (var site in siteList)
                {
                    _contentApiTrackingContext.Current.ReferencedSites.Add(new ReferencedSiteMetadata(site.Id, site.Saved));
                }

                // add default items to evict later on
                _contentApiTrackingContext.Current.ReferencedSites.Add(ReferencedSiteMetadata.DefaultInstance);

                return new ContentApiResult<IEnumerable<SiteDefinitionModel>>(siteList.Select(s => _siteDefinitionConverter.Convert(s, context)).ToList(), HttpStatusCode.OK, options, _contentApiSerializerResolver);
            }
            catch (ArgumentException argumentException)
            {
                _log.Error("argument exception", argumentException);
                return new ContentApiResult<ErrorResponse>(
                    new ErrorResponse(new Error(ErrorCode.InternalServerError, argumentException.Message)),
                    HttpStatusCode.InternalServerError);
            }
            catch (ContentNotFoundException notFoundException)
            {
                _log.Error("content not found exception", notFoundException);
                return new ContentApiResult<ErrorResponse>(
                    new ErrorResponse(new Error(ErrorCode.NotFound, notFoundException.Message)),
                    HttpStatusCode.NotFound);
            }
            catch (Exception exception)
            {
                _log.Error("internal server error", exception);
                return new ContentApiResult<ErrorResponse>(new ErrorResponse(new Error(ErrorCode.InternalServerError, exception.Message)),
                    HttpStatusCode.InternalServerError);
            }
        }

        [Route(VersionThreeSiteBase + "{id}")]
        [Route(VersionTwoSiteBase + "{id}")]
        [HttpGet]
        [HttpOptions]
        [ResponseType(typeof(SiteDefinitionModel))]
        public IHttpActionResult GetById(Guid id)
        {
            var options = _apiConfiguration.Default();

            try
            {
                var site = GetSite(id, options);

                if (site is object)
                {
                    _contentApiTrackingContext.Current.ReferencedSites.Add(new ReferencedSiteMetadata(site.Id, site.Saved));
                    return new ContentApiResult<SiteDefinitionModel>(_siteDefinitionConverter.Convert(site, CreateContext(options)), HttpStatusCode.OK, options, _contentApiSerializerResolver);
                }

                return NotFound();
            }
            catch (Exception exception)
            {
                _log.Error("internal server error", exception);
                return new ContentApiResult<ErrorResponse>(new ErrorResponse(new Error(ErrorCode.InternalServerError, exception.Message)),
                    HttpStatusCode.InternalServerError);
            }
        }

        private SiteDefinition GetSite(Guid id, ContentApiOptions options)
        {
            var currentSite = _currentSiteAccessor();

            if (id == currentSite.Id)
            {
                return currentSite;
            }

            return options.MultiSiteFilteringEnabled ? null : _siteDefinitionRepository.Get(id);
        }

        private ConverterContext CreateContext(ContentApiOptions contentApiOptions) =>
            new ConverterContext(contentApiOptions, null, null, true, CultureInfo.InvariantCulture, ContextMode.Default);
    }
}
