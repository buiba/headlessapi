using EPiServer.Cms.Shell.Service.Internal;
using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Core.Security.Internal;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using System.Globalization;

namespace EPiServer.ContentApi.Core.Serialization.Models
{
    /// <summary>
    /// base class for mapping between property models and property data based on PropertyContentReference 
    /// </summary>
    public partial class ContentReferencePropertyModelBase<T, U> : PersonalizablePropertyModel<T, U>,
                                                         IExpandableProperty<ContentApiModel>
                                                        where T : ContentModelReference
                                                        where U : PropertyContentReference
    {
        protected IContentLoader _contentLoader;
        protected readonly ContentLoaderService _contentLoaderService;
        private readonly ContentConvertingService _contentConvertingService;
        protected readonly IContentAccessEvaluator _accessEvaluator;
        protected readonly ISecurityPrincipal _principalAccessor;
        private readonly UrlResolverService _urlResolverService;
        protected U _propertyContentReference;

        internal ContentReferencePropertyModelBase() { }

        public ContentReferencePropertyModelBase(U propertyContentReference, ConverterContext converterContext)
            : this(propertyContentReference,
                   converterContext,
                 ServiceLocator.Current.GetInstance<ContentLoaderService>(),
                   ServiceLocator.Current.GetInstance<ContentConvertingService>(),
                   ServiceLocator.Current.GetInstance<IContentAccessEvaluator>(),
                   ServiceLocator.Current.GetInstance<ISecurityPrincipal>(),
                   ServiceLocator.Current.GetInstance<UrlResolverService>())
        {
        }



        public ContentReferencePropertyModelBase(
        U propertyContentReference,
        ConverterContext converterContext,
        ContentLoaderService contentLoaderService,
        ContentConvertingService contentConvertingService,
        IContentAccessEvaluator accessEvaluator,
        ISecurityPrincipal principalAccessor,
        UrlResolverService urlResolverService)
        : base(propertyContentReference, converterContext)
        {
            _contentLoaderService = contentLoaderService;
            _contentConvertingService = contentConvertingService;
            _accessEvaluator = accessEvaluator;
            _principalAccessor = principalAccessor;
            _urlResolverService = urlResolverService;
            _propertyContentReference = propertyContentReference;
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
            if (Value == null || ConverterContext.ShouldIgnoreExpandValue)
            {
                return;
            }

            ExpandedValue = ExtractExpandedValue(language);
        }

        /// <summary>
        /// Retrieve the expanded value from Value property. Normally, Value only contains some basic information of IContent (e.g. contentlink : {name, workid} ). 
        /// We can use this function to extract more information of IContent (e.g. contentlink: {name, workid, publishdate, editdate, etc}) from Value
        /// </summary>        
        protected virtual ContentApiModel ExtractExpandedValue(CultureInfo language)
        {
            var content = _contentLoaderService.Get(new ContentReference(Value.Id.Value, Value.WorkId.Value, Value.ProviderName), language.Name);
            if (content == null)
            {
                return null;
            }

            var principal = ExcludePersonalizedContent
                ? _principalAccessor.GetAnonymousPrincipal()
                : _principalAccessor.GetCurrentPrincipal();

            return !_accessEvaluator.HasAccess(content, principal, AccessLevel.Read)
                ? null
                : _contentConvertingService.ConvertToContentApiModel(content, new ConverterContext(ConverterContext));
        }

        /// <summary>
        /// Contain the property's expanded value 
        /// </summary>
        public virtual ContentApiModel ExpandedValue { get; set; }

        /// <summary>
        /// Get value from PropertyContentReference
        /// </summary>
        /// <returns></returns>
        protected virtual T GetValue()
        {
            return ContentReference.IsNullOrEmpty(_propertyContentReference.ContentLink) ? null : new ContentModelReference
            {
                Id = _propertyContentReference.ID,
                WorkId = _propertyContentReference.WorkID,
                GuidValue = _propertyContentReference.GuidValue,
                ProviderName = _propertyContentReference.ProviderName,
                Url = _urlResolverService.ResolveUrl(_propertyContentReference.ContentLink, null)
            } as T;
        }

        /// <inheritdoc />
        public override object Flatten()
        {
            if (Value is object)
            {
                Value.Expanded = ExpandedValue;
            }
            return Value;
        }
    }
}
