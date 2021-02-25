using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Security;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;
using EPiServer.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// base class for mapping between property models and property data based on PropertyContentReferenceList 
    /// </summary>
    public partial class ContentReferenceListPropertyModelBase<T, U> : PersonalizablePropertyModel<T, U>, IExpandableProperty<IEnumerable<ContentApiModel>>
        where T : List<ContentModelReference>
        where U : PropertyContentReferenceList
    {
        protected readonly IPermanentLinkMapper _linkMapper;
        protected readonly ContentLoaderService _contentLoaderService;
        private readonly ContentConvertingService _contentConvertingService;
        protected readonly IContentAccessEvaluator _accessEvaluator;
        protected readonly ISecurityPrincipal _principalAccessor;
        private readonly UrlResolverService _urlResolverService;
        protected readonly ConverterContext _converterContext;
        protected readonly U _propertyContentReferenceList;

        internal ContentReferenceListPropertyModelBase() { }

        public ContentReferenceListPropertyModelBase(
          U propertyContentReferenceList,
          ConverterContext converterContext)
            :this(propertyContentReferenceList, 
              converterContext,
              ServiceLocator.Current.GetInstance<IPermanentLinkMapper>(),
              ServiceLocator.Current.GetInstance<ContentLoaderService>(),
              ServiceLocator.Current.GetInstance<ContentConvertingService>(),
              ServiceLocator.Current.GetInstance<IContentAccessEvaluator>(),
              ServiceLocator.Current.GetInstance<ISecurityPrincipal>(),
              ServiceLocator.Current.GetInstance<UrlResolverService>())
        {}

        public ContentReferenceListPropertyModelBase(
           U propertyContentReferenceList,
           ConverterContext converterContext,
           IPermanentLinkMapper linkMapper,
           ContentLoaderService contentLoaderService,
           ContentConvertingService contentConvertingService,
           IContentAccessEvaluator accessEvaluator,
           ISecurityPrincipal principalAccessor,
           UrlResolverService urlResolverService)
            :base(propertyContentReferenceList, converterContext)
        {
            _linkMapper = linkMapper;
            _contentLoaderService = contentLoaderService;
            _contentConvertingService = contentConvertingService;
            _accessEvaluator = accessEvaluator;
            _principalAccessor = principalAccessor;
            _urlResolverService = urlResolverService;
            _converterContext = converterContext;
            _propertyContentReferenceList = propertyContentReferenceList;
            Value = GetValue();

            InitializeObsolete();
        }

        /// <summary>
        /// Retrieve the expanded value from Value property and set it to ExpandedValue property.
        /// Normally, a property only contains the basic information (e.g. contentlink : {name, workid} ). But for some special ones, 
        /// clients may want to get more information of it (e.g. contentlink: {name, workid, publishdate, editdate, etc}), 
        /// so in this case, we should call Expand() to get full information of property
        /// </summary>
        /// <param name="language"></param>
        public void Expand(CultureInfo language)
        {
            if (Value == null || !Value.Any() || ConverterContext.ShouldIgnoreExpandValue)
            {
                return;
            }
            
            ExpandedValue = ExtractExpandedValue(language);
        }

        /// <summary>
        /// Retrieve the expanded value from Value property. Normally, Value only contains some basic information of IContent (e.g. contentlink : {name, workid} ). 
        /// We can use this function to extract more information of IContent (e.g. contentlink: {name, workid, publishdate, editdate, etc}) from Value
        /// </summary>        
        protected virtual IEnumerable<ContentApiModel> ExtractExpandedValue(CultureInfo language)
        {
            var expandedValue = new List<ContentApiModel>();

            var contentReferences = Value.Select(x => new ContentReference(x.Id.Value, x.WorkId.Value, x.ProviderName));
            var content = _contentLoaderService.GetItemsWithOptions(contentReferences, language.Name).ToList();

            var principal = ExcludePersonalizedContent ? _principalAccessor.GetAnonymousPrincipal() : _principalAccessor.GetCurrentPrincipal();
            var filteredContent = content.Where(x => _accessEvaluator.HasAccess(x, principal, AccessLevel.Read)).ToList();
            filteredContent.ForEach(fc => expandedValue.Add(_contentConvertingService.ConvertToContentApiModel(fc, new ConverterContext(ConverterContext))));

            return expandedValue;
        }

        /// <inheritdoc />
        public virtual IEnumerable<ContentApiModel> ExpandedValue { get; set; }

        /// <summary>
        /// Get value from PropertyContentReferenceList
        /// </summary>
        /// <returns></returns>
        protected virtual T GetValue()
        {
            if (_propertyContentReferenceList.List == null || !_propertyContentReferenceList.List.Any())
            {
                return null;
            }

            return _propertyContentReferenceList.List.Select(x => new ContentModelReference
            {
                Id = x.ID,
                WorkId = x.WorkID,
                GuidValue = _linkMapper.Find(x)?.Guid ?? Guid.Empty,
                ProviderName = x.ProviderName,
                Url = _urlResolverService.ResolveUrl(x, null)
            }).ToList() as T;
        }

        /// <inheritdoc />
        public override object Flatten()
        {
            if (Value is null) return null;

            var value = Value.AsList();

            if (ExpandedValue is object)
            {
                var lookup = new Dictionary<Guid?, ContentApiModel>();
                foreach (var item in ExpandedValue)
                {
                    lookup[item.ContentLink.GuidValue] = item;
                }

                foreach (var contentLink in value)
                {
                    if (lookup.TryGetValue(contentLink.GuidValue, out var expanded))
                    {
                        contentLink.Expanded = expanded;
                    }
                }
            }

            return value;
        }
    }
}
