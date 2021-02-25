using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ValueProviders;
using EPiServer.ContentApi.Cms.Internal;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.ContentResult;
using EPiServer.ContentApi.Core.ContentResult.Internal;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Security.Internal;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Logging;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Newtonsoft.Json;
using IContentApiContextModeResolver = EPiServer.ContentApi.Core.IContextModeResolver;
using EPiServer.ContentApi.Core.Tracking;
using EPiServer.ContentApi.Core.OutputCache;
using EPiServer.ContentApi.Core.OutputCache.Internal;
using EPiServer.Globalization;
using System.Runtime.Remoting.Messaging;

namespace EPiServer.ContentApi.Cms.Controllers
{
    /// <summary>
    ///     Controller for returning requests for IContent from IContentLoader, with appropriate filtering
    /// </summary>
    [ContentApiAuthorization]
    [ContentApiCors]
    [CorsOptionsActionFilter]
    [ContentLanguageFilter]
    [OutputCacheFilter(new[] { DependencyTypes.Content })]
    public partial class ContentApiController : ApiController
    {
        private const string VersionTwoContentBase = RouteConstants.VersionTwoApiRoute + "content/";

        private static readonly ILogger _log = LogManager.GetLogger(typeof(ContentApiController));

        private readonly ContentLoaderService _contentLoaderService;
        private readonly IContentApiSiteFilter _siteFilter;
        private readonly IContentApiRequiredRoleFilter _requiredRoleFilter;
        private readonly ISecurityPrincipal _principalAccessor;
        private readonly IContentApiContextModeResolver _contextModeResolver;
        private readonly UserService _userService;
        private readonly ContentConvertingService _contentConvertingService;
        private readonly ContentApiAuthorizationService _authorizationService;
        private readonly ContentApiConfiguration _apiConfiguration;
        private readonly ContentApiSerializerResolver _contentSerializerResolver;
        private readonly IContentApiTrackingContextAccessor _contentApiTrackingContextAccessor;
        private readonly ContentResolver _contentResolver;
        private readonly ISiteDefinitionResolver _siteDefinitionResolver;
        private readonly IContentLanguageAccessor _contentLanguageAccessor;
        private readonly IUpdateCurrentLanguage _updateCurrentLanguage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentApiController"/> class.
        /// </summary>
        public ContentApiController() : this(
            ServiceLocator.Current.GetInstance<ContentConvertingService>(),
            ServiceLocator.Current.GetInstance<ContentLoaderService>(),
            ServiceLocator.Current.GetInstance<IContentApiSiteFilter>(),
            ServiceLocator.Current.GetInstance<IContentApiRequiredRoleFilter>(),
            ServiceLocator.Current.GetInstance<ISecurityPrincipal>(),
            ServiceLocator.Current.GetInstance<IContentApiContextModeResolver>(),
            ServiceLocator.Current.GetInstance<UserService>(),
            ServiceLocator.Current.GetInstance<ContentApiAuthorizationService>(),
            ServiceLocator.Current.GetInstance<ContentApiConfiguration>(),
            ServiceLocator.Current.GetInstance<ContentApiSerializerResolver>(),
            ServiceLocator.Current.GetInstance<IContentApiTrackingContextAccessor>(),
            ServiceLocator.Current.GetInstance<ContentResolver>(),
            ServiceLocator.Current.GetInstance<ISiteDefinitionResolver>(),
            ServiceLocator.Current.GetInstance<IContentLanguageAccessor>(),
            ServiceLocator.Current.GetInstance<IUpdateCurrentLanguage>())
        {
        }

        internal ContentApiController(
           ContentConvertingService contentConvertingService,
           ContentLoaderService contentLoaderService,
           IContentApiSiteFilter siteFilter,
           IContentApiRequiredRoleFilter requiredRoleFilter,
           ISecurityPrincipal principalAccessor,
           IContentApiContextModeResolver contextModeResolver,
           UserService userService,
           ContentApiAuthorizationService authorizationService,
           ContentApiConfiguration apiConfiguration,
           ContentApiSerializerResolver contentSerializerResolver,
           IContentApiTrackingContextAccessor contentApiTrackingContextAccessor,
           ContentResolver contentResolver,
           ISiteDefinitionResolver siteDefinitionResolver,
           IContentLanguageAccessor contentLanguageAccessor,
           IUpdateCurrentLanguage updateCurrentLanguage)
        {
            _contentLoaderService = contentLoaderService;
            _siteFilter = siteFilter;
            _requiredRoleFilter = requiredRoleFilter;
            _principalAccessor = principalAccessor;
            _contextModeResolver = contextModeResolver;
            _userService = userService;
            _contentConvertingService = contentConvertingService;
            _authorizationService = authorizationService;
            _apiConfiguration = apiConfiguration;
            _contentSerializerResolver = contentSerializerResolver;
            _contentApiTrackingContextAccessor = contentApiTrackingContextAccessor;
            _contentResolver = contentResolver;
            _siteDefinitionResolver = siteDefinitionResolver;
            _contentLanguageAccessor = contentLanguageAccessor;
            _updateCurrentLanguage = updateCurrentLanguage;
        }

        /// <summary>
        /// Get content by given content reference and language
        /// </summary>
        /// <param name="contentReference">Content reference to retrieve data</param>
        /// <param name="languages">Language used to retrieve content</param>
        /// <param name="expand">List of properties needed to be expanded. The list is separated by comma</param>
        /// <param name="select">List of properties needed to be returned. The list is separated by comma. Default are all properties returned</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        [Route(VersionTwoContentBase + "{contentReference}")]
        [HttpGet]
        [HttpOptions]
        [ResponseType(typeof(ContentApiModel))]
        public IHttpActionResult Get(string contentReference,
            [ValueProvider(typeof(AcceptLanguageHeaderValueProviderFactory))] List<string> languages,
            string expand = "",
            string select = "")
        {
            try
            {
                var language = languages?.FirstOrDefault();
                var content = _contentLoaderService.Get(new ContentReference(contentReference), language);

                return ResultFromContent(content, expand, select, false, ContextMode.Default, language: language);
            }
            catch (EPiServerException exception)
            {
                //to catch invalid content reference passed by user
                _log.Error(exception.Message, exception);
                return BuildResponseFromException(exception);
            }
            catch (Exception exception)
            {
                _log.Error("Error occurred during Content Api Get Request", exception);
                return BuildResponseFromException(exception);
            }
        }

        /// <summary>
        /// Get content by given content GUID and language
        /// </summary>
        /// <param name="contentGuid">ContentGuid of the content to retrieve data</param>
        /// <param name="languages">Language used to retrieve content</param>
        /// <param name="expand">List of properties needed to be expanded. The list is separated by comma</param>
        /// <param name="select">List of properties needed to be returned. The list is separated by comma. Default are all properties returned</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        [Route(VersionTwoContentBase + "{contentGuid:guid}")]
        [HttpGet]
        [HttpOptions]
        [ResponseType(typeof(ContentApiModel))]
        public IHttpActionResult GetContent(Guid contentGuid,
            [ValueProvider(typeof(AcceptLanguageHeaderValueProviderFactory))] List<string> languages,
            string expand = "",
            string select = "")
        {
            try
            {
                var language = languages?.FirstOrDefault();
                var content = _contentLoaderService.Get(contentGuid, language);

                return ResultFromContent(content, expand, select, false, ContextMode.Default, language: language);
            }
            catch (Exception exception)
            {
                _log.Error("Error occurred during Content Api GetContent Request", exception);
                return BuildResponseFromException(exception);
            }
        }

        ///<summary>
        /// Get the children of the content item with given language
        /// </summary>
        /// <param name="contentReference">Parent's content reference</param>
        /// <param name="languages">Language used to retrieve content</param>
        /// <param name="expand">List of properties needed to be expanded. The list is separated by comma</param>
        /// <param name="select">List of properties needed to be returned. The list is separated by comma. Default are all properties returned</param>
        /// <param name="top">The max number of children to return.</param>
        /// <param name="continuationToken">The max number of children to return.</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        [Route(VersionTwoContentBase + "{contentReference}/children")]
        [HttpGet]
        [HttpOptions]
        [ResponseType(typeof(ContentApiModel[]))]
        [OutputCacheFilter(new[] { DependencyTypes.Content, DependencyTypes.Children })]
        public IHttpActionResult GetChildren(string contentReference,
            [ValueProvider(typeof(AcceptLanguageHeaderValueProviderFactory))] List<string> languages,
            string expand = "",
            int? top = null,
            [FromHeader(Name = PagingConstants.ContinuationTokenHeaderName)] string continuationToken = "",
            string select = "")
        {
            if (!IsValidPagingParameters(top, continuationToken, false, out var pagingToken, out var errorResult))
            {
                return errorResult;
            }           

            return GetListResult(() =>
            {
                if (ShouldGetAllChildren(top, continuationToken))
                {
                    return _contentLoaderService.GetChildren(new ContentReference(contentReference), languages?.FirstOrDefault());
                }

                return _contentLoaderService.GetChildren(new ContentReference(contentReference), languages?.FirstOrDefault(), pagingToken,
                    c => HasAccessAndTrackFiltered(c, content => _authorizationService.CanUserAccessContent(content)));
            }, expand, select, pagingToken.Top);
        }

        /// <summary>
        /// Preview API: Get the children of the content item with given language
        /// </summary>
        /// <param name="contentGuid">Parent's guid based reference</param>
        /// <param name="languages">Language used to retrieve content</param>
        /// <param name="expand">List of properties needed to be expanded. The list is separated by comma</param>
        /// <param name="select">List of properties needed to be returned. The list is separated by comma. Default are all properties returned</param>
        /// <param name="top">The max number of children to return. Default value is 100 and that is also maximum value</param>
        /// <param name="continuationToken">The max number of children to return.</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        [Route(VersionTwoContentBase + "{contentGuid:guid}/children")]
        [HttpGet]
        [HttpOptions]
        [PreviewFeatureFilter]
        [ResponseType(typeof(IContentApiModel[]))]
        [OutputCacheFilter(new[] { DependencyTypes.Content, DependencyTypes.Children })]
        public IHttpActionResult GetChildren(Guid contentGuid,
            [ValueProvider(typeof(AcceptLanguageHeaderValueProviderFactory))] List<string> languages,
            string expand = "",
            string select = "",
            int? top = null,
            [FromHeader(Name = PagingConstants.ContinuationTokenHeaderName)] string continuationToken = "")
        {
            var usedTop = string.IsNullOrEmpty(continuationToken) ? (top ?? (int?)PagingConstants.DefaultChildrenBatchSize) : top;
            if (!IsValidPagingParameters(usedTop, continuationToken, true, out var pagingToken, out var errorResult))
            {
                return errorResult;
            }

            return GetListResult(() =>
            {
                return _contentLoaderService.GetChildren(contentGuid, languages?.FirstOrDefault(), pagingToken,
                    c => HasAccessAndTrackFiltered(c, content => _authorizationService.CanUserAccessContent(content)));
            },
            expand, select, pagingToken.Top);
        }

        /// <summary>
        /// Get the ancestors of the content item with given language
        /// </summary>
        /// <param name="contentReference">Content reference to retrieve ancestor</param>
        /// <param name="languages">Language used to retrieve content</param>
        /// <param name="expand">List of properties needed to be expanded. The list is separated by comma</param>
        /// <param name="select">List of properties needed to be returned. The list is separated by comma. Default are all properties returned</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        [Route(VersionTwoContentBase + "{contentReference}/ancestors")]
        [HttpGet]
        [HttpOptions]
        [ResponseType(typeof(ContentApiModel[]))]
        [OutputCacheFilter(new[] { DependencyTypes.Content, DependencyTypes.Ancestors })]
        public IHttpActionResult GetAncestors(string contentReference,
            [ValueProvider(typeof(AcceptLanguageHeaderValueProviderFactory))] List<string> languages,
            string expand = "",
            string select = "")
        {
            return GetListResult(() => _contentLoaderService.GetAncestors(new ContentReference(contentReference), languages?.FirstOrDefault()), expand, select);
        }

        /// <summary>
        /// Get the ancestors of the content item with given language
        /// </summary>
        /// <param name="contentGuid">Content guid based reference to retrieve ancestor</param>
        /// <param name="languages">Language used to retrieve content</param>
        /// <param name="expand">List of properties needed to be expanded. The list is separated by comma</param>
        /// <param name="select">List of properties needed to be returned. The list is separated by comma. Default are all properties returned</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>
        [Route(VersionTwoContentBase + "{contentGuid:guid}/ancestors")]
        [HttpGet]
        [HttpOptions]
        [ResponseType(typeof(ContentApiModel[]))]
        [OutputCacheFilter(new[] { DependencyTypes.Content, DependencyTypes.Ancestors })]
        public IHttpActionResult GetAncestors(Guid contentGuid,
            [ValueProvider(typeof(AcceptLanguageHeaderValueProviderFactory))] List<string> languages,
            string expand = "",
            string select = "")
        {
            return GetListResult(() => _contentLoaderService.GetAncestors(contentGuid, languages?.FirstOrDefault()), expand, select);
        }

        /// <summary>
        /// Get content by given content url or by list of GUID/Reference with given language
        /// </summary>
        /// <param name="languages">Language used to retrieve content</param>    
        /// <param name="guids">List of GUID seperated by ','</param>
        /// <param name="references">List of ContentReference seperated by ','</param>
        /// <param name="contentUrl">The absolute url to the content</param>
        /// <param name="expand">List of properties needed to be expanded. The list is separated by comma</param>
        /// <param name="select">List of properties needed to be returned. The list is separated by comma. Default are all properties returned</param>
        /// <param name="matchExact">Specifies if the specified <paramref name="contentUrl"/> should match url of the content exactly, that is no additional segments are allowed.</param>
        /// <remarks>
        /// If <paramref name="matchExact"/>"/> is set to false then it will route to the "nearest" content. Remaining segments are ignored. The url for the returned content can be
        /// used to determine if segments where ignored.
        /// Only use contentUrl or guids/references for seperate request. Use contentUrl for get content by url and guids for get content by list of GUID/Reference.
        /// </remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not Found</response>           
        [Route(VersionTwoContentBase)]
        [HttpGet]
        [HttpOptions]
        [ResponseType(typeof(ContentApiModel[]))]
        public IHttpActionResult QueryContent(
            [ValueProvider(typeof(AcceptLanguageHeaderValueProviderFactory))] List<string> languages,
            [FromUri] string contentUrl = "",
            [FromUri] string guids = "",
            [FromUri] string references = "",
            string expand = "",
            string select = "",
            bool matchExact = true)
        {
            try
            {
                if (!IsValidContentQueryParams(contentUrl, guids, references))
                {
                    throw new ArgumentException("Wrong parameter combination used. Only use contentUrl or guids/references");
                }

                if (!string.IsNullOrEmpty(contentUrl))
                {
                    return GetContentByUrl(contentUrl, expand, select, matchExact);
                }

                return GetItems(languages, guids ?? string.Empty, references ?? string.Empty, expand, select);
            }
            catch (Exception exception)
            {
                _log.Error("Error occurred during Content Api QueryContent Request", exception);
                return BuildResponseFromException(exception);
            }
        }

        public IHttpActionResult GetItems(
            [ValueProvider(typeof(AcceptLanguageHeaderValueProviderFactory))] List<string> languages,
            [FromUri] string guids = "",
            [FromUri] string references = "",
            string expand = "",
            string select = "")
        {
            var contents = new List<IContent>();
            var separators = new[] { ',' };
            try
            {
                var complexReferences = references.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                var contentGuids = guids.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                if (!complexReferences.Any() && !contentGuids.Any())
                {
                    return ResultFromContent(contents, expand, select, ContextMode.Default);
                }

                contents.AddRange(_contentLoaderService.GetItemsWithOptions(complexReferences.Select(x => new ContentReference(x)), languages?.FirstOrDefault()));
                contents.AddRange(_contentLoaderService.GetItemsWithOptions(contentGuids.Select(g => new Guid(g)), languages?.FirstOrDefault()));

                return ResultFromContent(contents, expand, select, ContextMode.Default);
            }
            catch (Exception exception)
            {
                _log.Error("Error occurred during Content Api GetItems Request", exception);
                return BuildResponseFromException(exception);
            }
        }

        // sample url to content in editmode /EPiServer/CMS/Content/en/alloy-meet,,13/?epieditmode=False
        private IHttpActionResult GetContentByUrl(
            [FromUri] string contentUrl,
            string expand = "",
            string select = "",
            bool matchExact = true)
        {
            try
            {
                var options = _apiConfiguration.Default();
                var content = _contentResolver.Resolve(contentUrl, matchExact, options.EnablePreviewMode);
                var contextMode = _contextModeResolver.Resolve(contentUrl, ContextMode.Default);
                var site = ResolveSite(contentUrl, content?.Content.ContentLink);
                var routeLanguage = content?.RouteLanguage;

                if (!string.IsNullOrEmpty(routeLanguage))
                {
                    _updateCurrentLanguage.UpdateLanguage(routeLanguage);
                }

                var (contentApiModel, header, statusCode) = ConvertContentToContentApiModel(content?.Content, expand, select, contextMode, site, content?.RemainingRoute, routeLanguage);
                return new ContentApiResult<IEnumerable<IContentApiModel>>((contentApiModel != null) ? new IContentApiModel[] { contentApiModel } : new IContentApiModel[0], statusCode, header, options, _contentSerializerResolver);
            }
            catch (Exception exception)
            {
                _log.Error("Error occurred during Content Api GetContentByUrl Request", exception);
                return BuildResponseFromException(exception);
            }
        }

        private IHttpActionResult GetListResult(Func<object> contentListLoader, string expand, string select, int? top = null)
        {
            try
            {
                var contentItems = contentListLoader();
                if (contentItems is IEnumerable<IContent> contentList)
                {
                    return ResultFromContent(contentList, expand, select, ContextMode.Default);
                }
                else if (contentItems is ContentDeliveryQueryRange<IContent> pagedContent)
                {
                    return ResultFromContent(pagedContent, top.GetValueOrDefault(), expand, select, ContextMode.Default);
                }
                else
                {
                    throw new InvalidOperationException($"GetListResult handles {nameof(IEnumerable<IContent>)} or {nameof(ContentDeliveryQueryRange<IContent>)}");
                }
            }            
            catch (Exception exception)
            {
                _log.Error($"Error occurred during request {Request.RequestUri}", exception);
                return BuildResponseFromException(exception);
            }
        }

        private IDictionary<string, string> GetMetadataHeaders(IContent content, ContextMode contextMode, SiteDefinition site, string remainingRoute, string routeLanguage)
        {
            var headers = new Dictionary<string, string>();

            if (content != null)
            {
                headers.Add(MetadataHeaderConstants.ContentGUIDMetadataHeaderName, content.ContentGuid.ToString());
            }

            if (!string.IsNullOrEmpty(routeLanguage))
            {
                headers.Add(MetadataHeaderConstants.BranchMetadataHeaderName, routeLanguage);
            }

            if (site is object)
            {
                var startPage = _contentLoaderService.Get(site.StartPage, routeLanguage, true);
                headers.Add(MetadataHeaderConstants.SiteIdMetadataHeaderName, site.Id.ToString());
                headers.Add(MetadataHeaderConstants.StartPageMetadataHeaderName, startPage.ContentGuid.ToString());
            }

            if (contextMode != ContextMode.Default)
            {
                headers.Add(MetadataHeaderConstants.ContextModeMetadataHeaderName, contextMode.ToString());
            }

            if (!string.IsNullOrEmpty(remainingRoute))
            {
                headers.Add(MetadataHeaderConstants.RemainingRouteMetadataHeaderName, remainingRoute);
            }

            return headers;
        }

        [Obsolete("This method will be removed soon when it is no used longer")]
        public IHttpActionResult GetItemsByGuids(
            [ValueProvider(typeof(AcceptLanguageHeaderValueProviderFactory))] List<string> languages,
            [FromUri] string guids = "",
            string expand = "",
            string select = "")
        {
            var contents = new List<IContent>();
            var separators = new[] { ',' };
            try
            {
                var contentGuids = guids.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                if (!contentGuids.Any())
                {
                    return ResultFromContent(contents, expand, select, ContextMode.Default);
                }

                contents.AddRange(_contentLoaderService.GetItemsWithOptions(contentGuids.Select(g => new Guid(g)), languages?.FirstOrDefault()));

                return ResultFromContent(contents, expand, select, ContextMode.Default);
            }
            catch (Exception exception)
            {
                _log.Error("Error occurred during Content Api GetItems Request", exception);
                return BuildResponseFromException(exception);
            }
        }

        // should we move a part of it to Core
        private IHttpActionResult ResultFromContent(IContent content, string expand, string select, bool addMetadataHeaders, ContextMode contextMode, SiteDefinition site = null, string remainingRoute = null, string language = null)
        {
            if (content == null || _siteFilter.ShouldFilterContent(content, SiteDefinition.Current))
            {
                throw new ContentNotFoundException();
            }

            if (_requiredRoleFilter.ShouldFilterContent(content))
            {
                throw new AccessDeniedException();
            }

            if (!_userService.IsUserAllowedToAccessContent(content, _principalAccessor.GetCurrentPrincipal(), AccessLevel.Read))
            {
                return new ContentApiResult<ErrorResponse>(
                    new ErrorResponse(new Error(ErrorCode.Forbidden, ErrorCode.Forbidden)),
                    HttpStatusCode.Forbidden);
            }

            var options = _apiConfiguration.Default();
            var contextLanguage = options.ExpandedBehavior == ExpandedLanguageBehavior.ContentLanguage ? (content as ILocale)?.Language : _contentLanguageAccessor.Language;
            var context = CreateContext(expand, select, contextLanguage, options, contextMode);
            var headers = addMetadataHeaders ? GetMetadataHeaders(content, contextMode, site, remainingRoute, language) : null;

            return new ContentApiResult<IContentApiModel>(_contentConvertingService.Convert(content, context), HttpStatusCode.OK, headers, options, _contentSerializerResolver);
        }

        private IHttpActionResult ResultFromContent(IEnumerable<IContent> contents, string expand, string select, ContextMode contextMode, bool addMetadataHeaders = false, SiteDefinition site = null, string remainingRoute = null, string routeLanguage = null)
        {
            var mappedContent = new List<IContentApiModel>();
            var options = _apiConfiguration.Default();
            foreach (var content in contents)
            {
                if (!HasAccessAndTrackFiltered(content, c => _authorizationService.CanUserAccessContent(c)))
                {
                    continue;
                }

                var contextLanguage = options.ExpandedBehavior == ExpandedLanguageBehavior.ContentLanguage ? (content as ILocale)?.Language : _contentLanguageAccessor.Language;
                var context = CreateContext(expand, select, contextLanguage, options, contextMode);
                mappedContent.Add(_contentConvertingService.Convert(content, context));
            }

            if (contents.Any() && addMetadataHeaders)
            {
                var headers = addMetadataHeaders ? GetMetadataHeaders(contents.FirstOrDefault(), contextMode, site, remainingRoute, routeLanguage) : null;
                return new ContentApiResult<IEnumerable<IContentApiModel>>(mappedContent, HttpStatusCode.OK, headers, options, _contentSerializerResolver);
            }

            return new ContentApiResult<IEnumerable<IContentApiModel>>(mappedContent, HttpStatusCode.OK, options, _contentSerializerResolver);
        }

        // This method is used specically for GetContentByUrl that need header metadata a lot
        private (IContentApiModel contentApiModel, IDictionary<string, string> header,  HttpStatusCode statusCode) ConvertContentToContentApiModel(IContent content, string expand, string select, ContextMode contextMode, SiteDefinition site = null, string remainingRoute = null, string routeLanguage = null)
        {
            var options = _apiConfiguration.Default();
            var statusCode = HttpStatusCode.OK;
            var headers = GetMetadataHeaders(content, contextMode, site, remainingRoute, routeLanguage);

            if (content == null)
            {
                return (null, headers, statusCode);
            }

            if (!_authorizationService.IsAnonymousAllowedToAccessContent(content) && !_principalAccessor.GetCurrentPrincipal().Identity.IsAuthenticated)
            {
                statusCode = HttpStatusCode.Unauthorized;
                headers.Remove(MetadataHeaderConstants.ContentGUIDMetadataHeaderName);
                return (null, headers, statusCode);
            }

            if (!HasAccessAndTrackFiltered(content, c => _authorizationService.CanUserAccessContent(c)))
            {
                statusCode = HttpStatusCode.Forbidden;
                headers.Remove(MetadataHeaderConstants.ContentGUIDMetadataHeaderName);
                return (null, headers, statusCode);
            }

            var contextLanguage = options.ExpandedBehavior == ExpandedLanguageBehavior.ContentLanguage ? (content as ILocale)?.Language : _contentLanguageAccessor.Language;
            var context = CreateContext(expand, select, contextLanguage, options, contextMode);
            return (_contentConvertingService.Convert(content, context), headers, statusCode);
        }

        private IHttpActionResult ResultFromContent(ContentDeliveryQueryRange<IContent> queryRange, int top, string expand, string selectProperties, ContextMode contextMode)
        {
            var options = _apiConfiguration.Default();

            var mappedContent = (from content in queryRange.PagedResult.PagedItems
                                 select _contentConvertingService.Convert(content, CreateContext(expand, selectProperties, options.ExpandedBehavior == ExpandedLanguageBehavior.ContentLanguage ? (content as ILocale)?.Language : _contentLanguageAccessor.Language, options, contextMode)))
                .ToArray();

            var headers = new Dictionary<string, string>();
            if (queryRange.HasMoreContent)
            {
                headers.Add(PagingConstants.ContinuationTokenHeaderName, ConvertToBase64(JsonConvert.SerializeObject(new PagingToken
                {
                    LastIndex = queryRange.LastIndex,
                    TotalCount = queryRange.PagedResult.TotalCount,
                    Top = top
                })));
            }

            return new ContentApiResult<IEnumerable<IContentApiModel>>(mappedContent, HttpStatusCode.OK, headers, options, _contentSerializerResolver);
        }

        private IHttpActionResult BuildResponseFromException(Exception exception)
        {
            //if value from _contentLoader.Get is not IContent or IContent does not exist
            if (exception.GetType() == typeof(TypeMismatchException) || exception.GetType() == typeof(ContentNotFoundException))
            {
                return new ContentApiResult<ErrorResponse>(
                    new ErrorResponse(new Error(ErrorCode.NotFound, ErrorMessage.NotFound)),
                    HttpStatusCode.NotFound);
            }

            if (exception.GetType() == typeof(ArgumentException))
            {
                return new ContentApiResult<ErrorResponse>(
                    new ErrorResponse(new Error(ErrorCode.InvalidParameter, exception.Message)),
                    HttpStatusCode.BadRequest);
            }

            //if language string passed to CultureInfo.GetCultureInfo is invalid
            if (exception.GetType() == typeof(ArgumentNullException) || exception.GetType() == typeof(CultureNotFoundException) || exception.GetType() == typeof(FormatException) || exception.GetType() == typeof(JsonReaderException))
            {
                return new ContentApiResult<ErrorResponse>(
                    new ErrorResponse(new Error(ErrorCode.InvalidHeaderValue, ErrorMessage.InvalidHeaderValue)),
                    HttpStatusCode.BadRequest);
            }

            //if current context principal does not have sufficient privileges on content
            if (exception.GetType() == typeof(AccessDeniedException))
            {
                return new ContentApiResult<ErrorResponse>(
                    new ErrorResponse(new Error(ErrorCode.Forbidden, ErrorMessage.Forbidden)),
                    HttpStatusCode.Forbidden);
            }
            
            if (exception is EPiServerException && exception.Message.StartsWith("ContentReference: Input string was not in a correct format"))
            {
                return new ContentApiResult<ErrorResponse>(
                    new ErrorResponse(new Error(ErrorCode.InvalidParameter, "The content reference is not in a valid format")),
                    HttpStatusCode.BadRequest);
            }

            _log.Error("Error occurred during Content Api Request", exception);

            return new ContentApiResult<ErrorResponse>(
                new ErrorResponse(new Error(ErrorCode.InternalServerError, ErrorMessage.InternalServerError)),
                HttpStatusCode.InternalServerError);
        }

        private bool IsValidPagingParameters(int? top, string continuationToken, bool validateUpperLimit, out PagingToken pagingToken, out IHttpActionResult errorResult)
        {
            errorResult = null;
            pagingToken = null;
            if (top.HasValue && top < 1)
            {
                errorResult = new ContentApiResult<ErrorResponse>(
                    new ErrorResponse(new Error(ErrorCode.InvalidParameter, "Top value is less than 1")),
                    HttpStatusCode.BadRequest);
                return false;
            }

            if (validateUpperLimit && top.GetValueOrDefault() > PagingConstants.DefaultChildrenBatchSize)
            {
                errorResult = new ContentApiResult<ErrorResponse>(
                      new ErrorResponse(new Error(ErrorCode.InvalidParameter, $"Max value for top parameter is {PagingConstants.DefaultChildrenBatchSize}")),
                      HttpStatusCode.BadRequest);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(continuationToken) && top.HasValue)
            {
                errorResult = new ContentApiResult<ErrorResponse>(
                    new ErrorResponse(new Error(ErrorCode.InvalidParameter, "Send both continuationToken and top value is not allowed")),
                    HttpStatusCode.BadRequest);
                return false;
            }

            try
            {
                pagingToken = !string.IsNullOrWhiteSpace(continuationToken) ?
                JsonConvert.DeserializeObject<PagingToken>(ConvertFromBase64(continuationToken))
                : new PagingToken
                {
                    Top = top.GetValueOrDefault()
                };
            }
            catch (Exception)
            {
                errorResult = new ContentApiResult<ErrorResponse>(
                  new ErrorResponse(new Error(ErrorCode.InvalidHeaderValue, ErrorMessage.InvalidHeaderValue)),
                  HttpStatusCode.BadRequest);
                return false;
            }

            return true;
        }

        private bool IsValidContentQueryParams(string contentUrl, string guids, string references = "")
        {
            if ((!string.IsNullOrEmpty(contentUrl) && !string.IsNullOrEmpty(guids)) || (!string.IsNullOrEmpty(references) && !string.IsNullOrEmpty(contentUrl)))
            {
                return false;
            }
            return true;
        }
        private IContentApiModel CreateContentModel(IContent content, ConverterContext converterContext) => _contentConvertingService.Convert(content, converterContext);

        private ConverterContext CreateContext(string expand, string select, CultureInfo language, ContentApiOptions contentApiOptions, ContextMode contextMode) =>
            new ConverterContext(contentApiOptions, select, expand, false, language, contextMode);

        private bool HasAccessAndTrackFiltered(IContent content, Func<IContent, bool> hasAccessEvaluator)
        {
            var hasAccess = hasAccessEvaluator(content);
            if (!hasAccess)
            {
                //It might be filtered for site or role, then we dont want to track
                if (!_authorizationService.IsAnonymousAllowedToAccessContent(content))
                {
                    _contentApiTrackingContextAccessor.Current.SecuredContent.Add(content.ContentLink);
                }
            }
            return hasAccess;
        }

        private SiteDefinition ResolveSite(string contentUrl, ContentReference contentLink)
        {
            if (Uri.TryCreate(contentUrl, UriKind.RelativeOrAbsolute, out var contentUri) && contentUri.IsAbsoluteUri)
            {
                return _siteDefinitionResolver.GetByHostname(contentUri.Authority, fallbackToWildcard: true);
            }
            else if (!ContentReference.IsNullOrEmpty(contentLink))
            {
                return _siteDefinitionResolver.GetByContent(contentLink, fallbackToWildcard: true);
            }
            return SiteDefinition.Current;
        }

        private static bool ShouldGetAllChildren(int? top, string continuationToken)
        {
            return !top.HasValue && string.IsNullOrWhiteSpace(continuationToken);
        }

        private static string ConvertToBase64(string plainText)
        {
            Validator.ThrowIfNull(nameof(plainText), plainText);

            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private static string ConvertFromBase64(string encodedData)
        {
            Validator.ThrowIfNull(nameof(encodedData), encodedData);

            var encodedBytes = Convert.FromBase64String(encodedData);
            return Encoding.UTF8.GetString(encodedBytes);
        }
    }
}
