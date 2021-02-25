using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ValueProviders;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.ContentResult;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Security.Internal;
using EPiServer.ContentApi.Search.Internal;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

namespace EPiServer.ContentApi.Search.Controllers
{

	/// <summary>
	/// Expose API endpoint for searching content
	/// </summary>
	[RoutePrefix(RouteConstants.VersionTwoApiRoute + "search/content")]
    [ContentApiAuthorization]
    [ContentApiCors]
    [CorsOptionsActionFilter]
    public class ContentApiSearchController : ApiController
    {
        private readonly IContentApiSearchProvider _searchProvider;
        private readonly ContentApiSearchConfiguration _searchConfig;
        private readonly ContentApiConfiguration _contentApiConfiguration;
        private readonly ILogger _log = LogManager.GetLogger(typeof(ContentApiSearchController));
        private readonly ContentApiSerializerResolver _contentApiSerializerResolver;

        public ContentApiSearchController() : this(ServiceLocator.Current.GetInstance<IContentApiSearchProvider>(),
                                                    ServiceLocator.Current.GetInstance<ContentApiSearchConfiguration>(),
                                                    ServiceLocator.Current.GetInstance<ContentApiConfiguration>(),
                                                    ServiceLocator.Current.GetInstance<ContentApiSerializerResolver>())
        {
        }

        [Obsolete("Use alternative constructor")]
        public ContentApiSearchController(IContentApiSearchProvider searchProvider, ContentApiSearchConfiguration searchConfig) :
            this(searchProvider,
                 searchConfig,
                 ServiceLocator.Current.GetInstance<ContentApiConfiguration>(),
                 ServiceLocator.Current.GetInstance<ContentApiSerializerResolver>())
        {
        }

        internal ContentApiSearchController(IContentApiSearchProvider searchProvider,
            ContentApiSearchConfiguration searchConfig,
            ContentApiConfiguration contentApiConfiguration,
            ContentApiSerializerResolver contentApiSerializerResolver)
        {
            _searchProvider = searchProvider;
            _searchConfig = searchConfig;
            _contentApiConfiguration = contentApiConfiguration;
            _contentApiSerializerResolver = contentApiSerializerResolver;
        }

        /// <summary>
        /// Search contents based on criteria 
        /// </summary>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        [Route("")]
        [HttpGet]
        [HttpOptions]
        [ResponseType(typeof(SearchResponse))]
        public IHttpActionResult Search([FromUri]SearchRequest request, [ValueProvider(typeof(AcceptLanguageHeaderValueProviderFactory))] List<string> languages)
        {
            try
            {
                Error error = null;
                var options = _searchConfig.GetSearchOptions();

                if (request == null)
                {
                    request = new SearchRequest(_searchConfig);
                }

                if (!RequestIsValid(request, out error))
                {
                    return new ContentApiErrorResult(error, HttpStatusCode.BadRequest);
                }

                if (request.Top > options.MaximumSearchResults)
                {
                    request.Top = options.MaximumSearchResults;
                }

                var result = _searchProvider.Search(request, languages) ?? new SearchResponse()
                {
                    Results = null,
                    TotalMatching = 0
                };

                return new ContentApiResult<SearchResponse>(result, HttpStatusCode.OK, _contentApiConfiguration.Default(), _contentApiSerializerResolver);
            }
            catch (OrderByParseException ex)
            {
                _log.Error("Error occurred during Content Api Search Request", ex);
                return new ContentApiErrorResult(new Error(ErrorCode.InvalidOrderByClause, ErrorMessage.InvalidOrderByClause), HttpStatusCode.BadRequest);
            }
            catch (FilterParseException ex)
            {
                _log.Error("Error occurred during Content Api Search Request", ex);
                return new ContentApiErrorResult(new Error(ErrorCode.InvalidFilterClause, ErrorMessage.InvalidFilterClause), HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _log.Error("Error occurred during Content Api Search Request", ex);
                return new ContentApiErrorResult(new Error(ErrorCode.InternalServerError, ErrorMessage.InternalServerError), 
                    HttpStatusCode.InternalServerError);
            }

        }

        private bool RequestIsValid(SearchRequest request, out Error error)
        {
            if (request.Top < 1)
            {
                error = new Error(ErrorCode.InputOutOfRange, "Top value must be greater than 0");
                return false;
            }
            
            if (request.Skip < 0)
            {
                error = new Error(ErrorCode.InputOutOfRange, "Skip value must be greater than or equal to zero");
                return false;
            }

            error = null;
            return true;
        }
    }
}
