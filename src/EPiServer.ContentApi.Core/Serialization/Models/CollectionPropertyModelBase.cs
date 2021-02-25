using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentApi.Core.Security;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// propertymodel base for PropertyLongString
    /// </summary>
    public partial class CollectionPropertyModelBase<T, U> : PersonalizablePropertyModel<IEnumerable<T>, U>, IExpandableProperty<IEnumerable<ContentApiModel>>
                                                 where T : IContentItem
                                                 where U : PropertyLongString
    {
        protected readonly ContentLoaderService _contentLoaderService;
        private readonly ContentConvertingService _contentConvertingService;
        protected readonly IContentAccessEvaluator _accessEvaluator;
        protected readonly ISecurityPrincipal _principalAccessor;
        protected U _propertyLongString;

        internal CollectionPropertyModelBase() { }

        public CollectionPropertyModelBase(U propertyLongString, ConverterContext converterContext)
            : this(
                 propertyLongString,
                 converterContext,
                 ServiceLocator.Current.GetInstance<ContentLoaderService>(),
                 ServiceLocator.Current.GetInstance<ContentConvertingService>(),
                 ServiceLocator.Current.GetInstance<IContentAccessEvaluator>(),
                 ServiceLocator.Current.GetInstance<ISecurityPrincipal>())
        {

        }

        public CollectionPropertyModelBase(
            U propertyLongString,
            ConverterContext converterContext,
            ContentLoaderService contentLoaderService,
            ContentConvertingService contentConvertingService,
            IContentAccessEvaluator accessEvaluator,
            ISecurityPrincipal principalAccessor) : base(propertyLongString, converterContext)
        {
            //To ensure obosoleted mebers are initialized
            InitializeObsolete();
            _propertyLongString = propertyLongString;
            _contentLoaderService = contentLoaderService;
            _contentConvertingService = contentConvertingService;
            _accessEvaluator = accessEvaluator;
            _principalAccessor = principalAccessor;
            Value = GetValue();
        }

        /// <summary>
        /// Contains expanded value returned to clients
        /// </summary>
        public virtual IEnumerable<ContentApiModel> ExpandedValue { get; set; }

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

            var contentReferences = Value.Where(x => x.ContentLink != null).Select(x => new ContentReference(x.ContentLink.Id.Value, x.ContentLink.WorkId.Value, x.ContentLink.ProviderName));
            var content = _contentLoaderService.GetItemsWithOptions(contentReferences, language.Name).ToList();

            var principal = ExcludePersonalizedContent ? _principalAccessor.GetAnonymousPrincipal() : _principalAccessor.GetCurrentPrincipal();
            var filteredContent = content.Where(x => _accessEvaluator.HasAccess(x, principal, AccessLevel.Read)).ToList();
            filteredContent.ForEach(fc => expandedValue.Add(_contentConvertingService.ConvertToContentApiModel(fc, new ConverterContext(ConverterContext))));

            return expandedValue;
        }

        /// <summary>
        /// Get value from property long string
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<T> GetValue()
        {
            return Value;
        }

        /// <inheritdoc />
        public override object Flatten()
        {
            if (Value is null) return null;

            var value = Value.Where(v => v.ContentLink != null).AsList();

            if (ExpandedValue is object)
            {
                var lookup = new Dictionary<Guid?, ContentApiModel>();
                foreach (var item in ExpandedValue)
                {
                    lookup[item.ContentLink.GuidValue] = item;
                }

                foreach (var contentLink in value.Select(x => x.ContentLink))
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
