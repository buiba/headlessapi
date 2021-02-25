using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Api.Querying;
using EPiServer.Find.Cms;
using EPiServer.Security;
using Newtonsoft.Json.Linq;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.ServiceLocation;
using System.Globalization;

namespace EPiServer.ContentApi.Search.Internal
{
    /// <summary>
    ///     Implementation of <see cref="IContentApiSearchProvider"/> which uses Episerver Find as the search provider
    /// </summary>
    public class FindContentApiSearchProvider : IContentApiSearchProvider
    {
        protected readonly IClient _searchClient;
        protected readonly ContentConvertingService _contentConvertingService;
        protected readonly ContentApiSearchConfiguration _searchConfig;
        protected readonly ContentApiConfiguration _contentApiConfig;
        protected readonly IFindODataParser _parser;
        protected readonly RoleService _roleService;

        [Obsolete("Use alternative constructor")]
        public FindContentApiSearchProvider(
            IClient searchClient,
            IContentModelMapperFactory contentModelMapperFactory,
            ContentApiSearchConfiguration searchConfig,
            ContentApiConfiguration contentApiConfig,
            IFindODataParser parser,
            RoleService roleService)
            :this(searchClient, 
                 ServiceLocator.Current.GetInstance<ContentConvertingService>(),
                 searchConfig,
                 contentApiConfig, 
                 parser,
                 roleService)
        {}

        public FindContentApiSearchProvider(
          IClient searchClient,
          ContentConvertingService contentConvertingService,
          ContentApiSearchConfiguration searchConfig,
          ContentApiConfiguration contentApiConfig,
          IFindODataParser parser,
          RoleService roleService)
        {
            _searchClient = searchClient;
            _contentConvertingService = contentConvertingService;

            _searchConfig = searchConfig;
            _contentApiConfig = contentApiConfig;

            _parser = parser;
            _roleService = roleService;
        }

        public SearchResponse Search(SearchRequest searchRequest, IEnumerable<string> languages)
        {
            var options = _contentApiConfig.GetOptions();

            var search = _searchClient.Search<IContent>();
            if (search == null || searchRequest == null)
            {
                return null;
            }

            if (!searchRequest.Query.IsNullOrEmpty())
            {
                search = AttachQuery(search, searchRequest.Query);
            }

            if (!searchRequest.Filter.IsNullOrEmpty())
            {
                search = AttachFilters(search, searchRequest.Filter);
            }

            search = AttachSkip(search, searchRequest.Skip);
            search = AttachTop(search, searchRequest.Top);
            search = AttachPublishedFilter(search);
            search = AttachReadAccessFilter(search);

            if (!string.IsNullOrWhiteSpace(options.RequiredRole))
            {
                search = AttachContentApiRoleFilter(search, options.RequiredRole);
            }

            if (options.MultiSiteFilteringEnabled)
            {
                search = AttachCurrentSiteFilter(search);
            }

            if (languages != null && languages.Any())
            {
                search = AttachLanguageFilters(search, languages);
            }

            var searchOptions = _searchConfig.GetSearchOptions();
            if (searchOptions.SearchCacheDuration > TimeSpan.Zero)
            {
                search = AttachResponseCaching(search, searchOptions.SearchCacheDuration);
            }

            if (!searchRequest.OrderBy.IsNullOrEmpty())
            {
                search = AttachOrderBy(search, searchRequest.OrderBy);
            }

            return (!searchRequest.Personalize) ? ReturnResultsViaIndex(search, searchRequest.Expand)
                                                : ReturnResultsViaDatabase(search, searchRequest.Expand);
        }
        public virtual SearchResponse ReturnResultsViaDatabase(ITypeSearch<IContent> search, string expand)
        {
            var findResults = search.GetContentResult();
            return new SearchResponse()
            {
                Results = findResults?.Select(content =>
                {
                    return _contentConvertingService.ConvertToContentApiModel(content, new ConverterContext(_contentApiConfig.Default(), string.Empty, expand, false, (content as ILocale)?.Language));
                }),
                TotalMatching = (findResults == null) ? 0 : findResults.TotalMatching
            };
        }

        public virtual SearchResponse ReturnResultsViaIndex(ITypeSearch<IContent> search, string expand)
        {
            var searchResults = search.Select(x => x.ContentApiModel())?.GetResult();
            if (searchResults == null)
            {
                return null;
            }

            var returnList = new List<ContentApiModel>();
            //Find results contain all properties expanded - remove what we shouldn't send over the wire
            var propertiesToExpand = (string.IsNullOrWhiteSpace(expand) ? null : expand.Split(',').Select(x => x.ToLowerInvariant())) ?? new List<string>();

            var foundItems = searchResults.Hits?.Select(h => h.Document).Where(d => d != null);
            if (foundItems == null || !foundItems.Any())
            {
                return new SearchResponse()
                {
                    Results = null,
                    TotalMatching = searchResults.TotalMatching
                };
            }

            foreach (var item in foundItems)
            {
                if (item.Properties == null)
                {
                    continue;
                }

                var newDictionary = new Dictionary<string, object>();
                foreach (var property in item.Properties)
                {
                    if (property.Key == "$type")
                    {
                        continue;
                    }

                    if (propertiesToExpand.Contains(property.Key.ToLowerInvariant()) || string.Equals(expand, "*", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!newDictionary.ContainsKey(property.Key))
                        {
                            newDictionary.Add(property.Key, property.Value);
                        }
                        continue;
                    }

                    JObject jObject = property.Value as JObject;
                    if (jObject == null)
                    {
                        continue;
                    }

                    jObject.Remove("expandedValue");
                    if (!newDictionary.ContainsKey(property.Key))
                    {
                        newDictionary.Add(property.Key, jObject);
                    }
                }

                item.Properties = newDictionary;
                returnList.Add(item);
            }

            return new SearchResponse()
            {
                Results = returnList,
                TotalMatching = searchResults.TotalMatching
            };
        }
        public virtual ITypeSearch<IContent> AttachQuery(ITypeSearch<IContent> search, string query)
        {
            return search.For(query);
        }

        public virtual ITypeSearch<IContent> AttachResponseCaching(ITypeSearch<IContent> search,
            TimeSpan searchCacheDuration)
        {
            return search.StaticallyCacheFor(searchCacheDuration);
        }

        public virtual ITypeSearch<IContent> AttachLanguageFilters(ITypeSearch<IContent> search, IEnumerable<string> languages)
        {
            return search.FilterOnLanguages(languages);
        }
        public virtual ITypeSearch<IContent> AttachReadAccessFilter(ITypeSearch<IContent> search)
        {
            return search.FilterOnReadAccess();
        }

        public virtual ITypeSearch<IContent> AttachTop(ITypeSearch<IContent> search, int searchRequestTop)
        {
            return search.Take(searchRequestTop);
        }

        public virtual ITypeSearch<IContent> AttachSkip(ITypeSearch<IContent> search, int searchRequestSkip)
        {
            return search.Skip(searchRequestSkip);
        }

        public virtual ITypeSearch<IContent> AttachPublishedFilter(ITypeSearch<IContent> search)
        {
            return search.CurrentlyPublished();
        }
        public virtual ITypeSearch<IContent> AttachFilters(ITypeSearch<IContent> search, string filter)
        {
            return search.Filter(_parser.ParseFilter(filter));
        }
        public virtual ITypeSearch<IContent> AttachOrderBy(ITypeSearch<IContent> search, string orderby)
        {
            return new Search<IContent, IQuery>(search,
                context => _parser.ParseOrderBy(orderby).ToList().ForEach(x => context.RequestBody.Sort.Add(x)));
        }
        public virtual ITypeSearch<IContent> AttachCurrentSiteFilter(ITypeSearch<IContent> search)
        {
            return search.FilterOnCurrentSite();
        }

        /// <summary>
        /// Build filter for EPiServer Find
        /// </summary>
        public virtual ITypeSearch<IContent> AttachContentApiRoleFilter(ITypeSearch<IContent> search, string role)
        {
            var mappedroles = GetMappedRoles(role);

            var filter = BuildContentSecurableFilter(mappedroles);

            return search.Filter(filter);
        }

        protected virtual IEnumerable<string> GetMappedRoles(string role)
        {
            var mappedroles = _roleService.GetMappedRolesAssociatedWithVirtualRole(role)?.ToList() ?? new List<string>();

            if (!mappedroles.Any(x => x == role))
            {
                mappedroles.Add(role);
            }

            return mappedroles;
        }

        protected virtual FilterBuilder<IContentSecurable> BuildContentSecurableFilter(IEnumerable<string> mappedRoles)
        {
            var filter = _searchClient.BuildFilter<IContentSecurable>();
            foreach (var item in mappedRoles)
            {
                filter = filter.Or(x => x.RolesWithReadAccess().Match(item));
            }

            return filter;
        }

    }
}
