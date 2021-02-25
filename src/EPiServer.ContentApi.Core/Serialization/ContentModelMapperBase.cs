using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework;
using EPiServer.Web.Routing;
using Newtonsoft.Json;

namespace EPiServer.ContentApi.Core.Serialization
{
    /// <summary>
    /// Base class for model mappers
    /// </summary>
    public abstract partial class ContentModelMapperBase : IContentConverter
    {
        protected readonly IContentTypeRepository _contentTypeRepository;
        protected readonly IContentModelReferenceConverter _contentModelService;
        protected readonly ReflectionService _reflectionService;
        protected readonly IContentVersionRepository _contentVersionRepository;
        protected readonly ContentLoaderService _contentLoaderService;
        protected readonly UrlResolverService _urlResolverService;
        protected readonly IUrlResolver _urlResolver;
        private readonly ContentApiConfiguration _apiConfig;
        private readonly IPropertyConverterResolver _propertyConverterResolver;

        /// <exclude/> //for mock
        protected ContentModelMapperBase() { }

        public ContentModelMapperBase(
            IContentTypeRepository contentTypeRepository,
            ReflectionService reflectionService,
            IContentModelReferenceConverter contentModelService,
            IContentVersionRepository contentVersionRepository,
            ContentLoaderService contentLoaderService,
            UrlResolverService urlResolverService,
            ContentApiConfiguration apiConfig,
            IPropertyConverterResolver propertyConverterResolver)
        {
            _contentTypeRepository = contentTypeRepository;
            _reflectionService = reflectionService;
            _contentModelService = contentModelService;
            _contentVersionRepository = contentVersionRepository;
            _contentLoaderService = contentLoaderService;
            _urlResolverService = urlResolverService;
            _apiConfig = apiConfig;
            _propertyConverterResolver = propertyConverterResolver;
        }

        /// <inheritdoc />
        public virtual ContentApiModel Convert(IContent content, ConverterContext converterContext)
        {
            Validator.ThrowIfNull(nameof(content), content);

            var contentModel = converterContext.IsContentManagementRequest
                ? CreateDefaultModel(content, converterContext)
                : CreateDefaultModel(content);

            ExtractContentProperties(content, contentModel, converterContext);

            if (ExtractCustomProperties(content, converterContext) is Dictionary<string, object> propertyMap && propertyMap.Keys.Any())
            {
                contentModel.Properties = contentModel.Properties.Concat(propertyMap).ToDictionary(x => x.Key, x => x.Value);
            }

            if (converterContext.Options.FlattenPropertyModel)
            {
                FlattenPropertyMap(contentModel);
            }

            return contentModel;
        }

        /// <summary>
        /// Create default model corresponding with the content
        /// </summary>
        protected virtual ContentApiModel CreateDefaultModel(IContent content)
        {
            return CreateDefaultModel(content, null);
        }

        /// <summary>
        /// Extract content properties to ContentApiModel
        /// </summary>
        protected virtual void ExtractContentProperties(IContent content, ContentApiModel contentModel, ConverterContext converterContext)
        {
            contentModel.ContentType = GetAllContentTypes(content).ToList();
            contentModel.Language = ExtractLocalLanguage(content);
            contentModel.MasterLanguage = ExtractMasterLanguage(content);
            contentModel.ExistingLanguages = ExtractExistingLanguages(content, converterContext).ToList();

            var versionable = content as IVersionable;
            if (versionable != null)
            {
                contentModel.StartPublish = versionable.StartPublish?.ToUniversalTime();
                contentModel.StopPublish = versionable.StopPublish?.ToUniversalTime();
                contentModel.Status = versionable.Status.ToString();
            }

            var changeTracking = content as IChangeTrackable;
            if (changeTracking != null)
            {
                contentModel.Created = changeTracking.Created.ToUniversalTime();
                contentModel.Changed = changeTracking.Changed.ToUniversalTime();
                contentModel.Saved = changeTracking.Saved.ToUniversalTime();
            }

            var routeable = content as IRoutable;
            if (routeable != null)
            {
                contentModel.RouteSegment = routeable.RouteSegment;
            }

            contentModel.Url = ResolveUrl(content.ContentLink, null);

            var categorizable = content as ICategorizable;
            if (categorizable != null)
            {
                var propertyCategory = new PropertyCategory(categorizable.Category);
                var categoryPropertyModel = new CategoryPropertyModel(propertyCategory);
                AddToPropertyMap(contentModel.Properties, "Category", categoryPropertyModel);
            }
        }

        /// <summary>
        /// Extract local languages of a specific content
        /// </summary>
        protected virtual LanguageModel ExtractLocalLanguage(IContent content)
        {
            var localeData = content as ILocale;
            if (localeData == null || localeData.Language == null)
            {
                return null;
            }

            return new ContentLanguageModel()
            {
                DisplayName = localeData.Language.DisplayName,
                Name = localeData.Language.Name,
                Link = ResolveUrl(content.ContentLink, localeData.Language.Name)
            };
        }

        /// <summary>
        /// Extract master languages of a specific content
        /// </summary>
        protected virtual LanguageModel ExtractMasterLanguage(IContent content)
        {
            var languageData = content as ILocalizable;
            if (languageData == null || languageData.MasterLanguage == null)
            {
                return null;
            }

            return new ContentLanguageModel()
            {
                DisplayName = languageData.MasterLanguage.DisplayName,
                Name = languageData.MasterLanguage.Name,
                Link = ResolveUrl(content.ContentLink.ToReferenceWithoutVersion(), languageData.MasterLanguage.Name)
            };
        }

        /// <summary>
        /// Extract existing languages of a specific content
        /// </summary>
        protected virtual IEnumerable<LanguageModel> ExtractExistingLanguages(IContent content)
        {
            return ExtractExistingLanguages(content, null);
        }

        /// <summary>
        /// Extract existing languages of a specific content and Converter context
        /// </summary>
        protected virtual IEnumerable<LanguageModel> ExtractExistingLanguages(IContent content,
            ConverterContext converterContext)
        {
            var languageData = content as ILocalizable;
            if (languageData == null || languageData.ExistingLanguages == null)
            {
                return Enumerable.Empty<LanguageModel>();
            }

            //if request came from Content Management endpoint, with un-published contents, do NOT filter
            if (converterContext is object && converterContext.IsContentManagementRequest)
            {
                return languageData.ExistingLanguages.Select(x => new ContentLanguageModel()
                {
                    DisplayName = x.DisplayName,
                    Name = x.Name,
                    Link = ResolveUrl(content.ContentLink.ToReferenceWithoutVersion(), x.Name)
                });
            }

            var publishedContents = _contentVersionRepository.ListPublished(content.ContentLink);
            var languagesWithPublishedContents = publishedContents.Select(x => x.LanguageBranch);

            // Filter the existingLanguages list to make it contain only languages which has a published version
            // In case the contentLink contain version ID (Ex: 84_100), the urlResolver will ignore the language param. So we should use
            // ContentLink.ToReferenceWithoutVersion() here to make sure we always get the correct link of each language
            return languageData.ExistingLanguages.Where(x => languagesWithPublishedContents.Contains(x.Name)).Select(x => new ContentLanguageModel()
            {
                DisplayName = x.DisplayName,
                Name = x.Name,
                Link = ResolveUrl(content.ContentLink.ToReferenceWithoutVersion(), x.Name)
            });
        }

        /// <summary>
        /// Resolve url by content link and language. By default, this function always resolve url using view mode context
        /// </summary>
        protected virtual string ResolveUrl(ContentReference contentLink, string language)
        {
            // This is a hack to prevent breaking change.
            // If the old _urlResolver exists then we use it to resolve url, otherwise we use our new url resolver service
            if (_urlResolver != null)
            {
                return _urlResolver.GetUrl(contentLink, language, new UrlResolverArguments
                {
                    ContextMode = Web.ContextMode.Default,
                    ForceCanonical = true,
                });
            }

            return _urlResolverService.ResolveUrl(contentLink, language);
        }

        /// <summary>
        /// Get all content types of a specific content
        /// </summary>
        protected virtual IEnumerable<string> GetAllContentTypes(IContent content)
        {
            var baseTypes = new List<string>();
            var contentType = GetContentTypeById(content.ContentTypeID);

            if (contentType.Base != ContentTypeBase.Undefined)
            {
                baseTypes.Add(contentType.Base.ToString());
            }

            if (content is BlockData)
            {
                baseTypes.Add("Block");
            }
            else if (content is PageData)
            {
                baseTypes.Add("Page");
            }
            else if (content is MediaData)
            {
                baseTypes.Add("Media");

                if (content is VideoData)
                {
                    baseTypes.Add("Video");
                }
                else if (content is ImageData)
                {
                    baseTypes.Add("Image");
                }
            }

            baseTypes.Add(contentType.Name);

            return baseTypes.Distinct();
        }

        /// <summary>
        /// Get content type by id
        /// </summary>
        protected virtual ContentType GetContentTypeById(int contentTypeId)
        {
            var contentType = _contentTypeRepository.Load(contentTypeId);
            if (contentType == null)
            {
                throw new Exception($"Content Type id {contentTypeId} not found.");
            }

            return contentType;
        }

        /// <summary>
        /// Extract property data collection from content
        /// </summary>
        protected virtual IDictionary<string, object> ExtractCustomProperties(IContent content, ConverterContext contentMappingContext)
        {
            var propertyMap = new Dictionary<string, object>();
            var languageData = content as ILocalizable;

            var contentType = GetContentTypeById(content.ContentTypeID);

            foreach (var property in content.Property.Where(x => !x.IsMetaData))
            {
                if (content is object && ShouldPropertyBeIgnored(contentType, property))
                {
                    continue;
                }

                var converter = _propertyConverterResolver.Resolve(property);
                if (converter is object)
                {
                    //if current property need to be expand, call GetValue with expand paramerter set to true.
                    var propertyModel = converter.Convert(property, contentMappingContext);
                    AddToPropertyMap(propertyMap, property.Name, propertyModel);
                }
            }

            return propertyMap;
        }

        /// <summary>
        /// Add propertyModel to propertyMap of ContentApiModel. 3rd developer may override this method to add their own customization
        /// with properyModel before add to ContentApiModel
        /// </summary>
        protected virtual void AddToPropertyMap(IDictionary<string, object> propertyMap, string key, IPropertyModel propertyModel)
        {
            propertyMap.Add(key, propertyModel);
        }

        /// <summary>
        /// Decide whether a content type's property should be ignored from data serialization
        /// </summary>
        protected virtual bool ShouldPropertyBeIgnored(ContentType contentType, PropertyData property)
        {
            var propertyAttributes = _reflectionService.GetAttributes(contentType, property);
            return propertyAttributes != null && propertyAttributes.OfType<JsonIgnoreAttribute>().Any();
        }

        /// <summary>
        /// Flattens the property map on the content model
        /// </summary>
        protected virtual void FlattenPropertyMap(ContentApiModel contentModel)
        {
            var flattened = new Dictionary<string, object>(contentModel.Properties.Count);

            foreach (var item in contentModel.Properties)
            {
                if (item.Value is IFlattenableProperty property)
                {
                    flattened[item.Key] = property.Flatten();
                }
                else
                {
                    flattened[item.Key] = item.Value;
                }
            }

            contentModel.Properties = flattened;
        }

        /// <summary>
        /// Create default model corresponding with the content
        /// </summary>
        private ContentApiModel CreateDefaultModel(IContent content, ConverterContext converterContext)
        {
            var parent = converterContext != null && converterContext.IsContentManagementRequest
                ? _contentLoaderService.Get(content.ParentLink, (content as ILocalizable)?.Language, alwaysExposeContent:true)
                : _contentLoaderService.Get(content.ParentLink, (content as ILocalizable)?.Language?.Name, fallbackToMaster:true);

            return new ContentApiModel
            {
                // url should follow content language
                ContentLink = _contentModelService.GetContentModelReference(content),
                Name = content.Name,
                // parent link should follow header language and the method (overload of function above for ContentLink) below guarantee that
                ParentLink = (parent != null) ? _contentModelService.GetContentModelReference(parent.ContentLink) : null

            };
        }
    }
}
