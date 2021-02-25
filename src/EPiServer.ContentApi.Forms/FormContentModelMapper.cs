using System.Collections.Generic;
using System.Web;
using EPiServer.ContentApi.Core;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.ContentApi.Forms.Model;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Forms;
using EPiServer.Forms.Configuration;
using EPiServer.Forms.Core;
using EPiServer.Forms.Implementation.Elements;
using EPiServer.Framework.Configuration;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPiServer.ContentApi.Forms
{
    /// <summary>
    /// Mapper to handle form content type
    /// The mapper transforms form data into FormContentApiModel for serialization.
    /// </summary>
    [ServiceConfiguration]
    public partial class FormContentModelMapper : ContentModelMapperBase
    {
        /// <summary>
        ///     Initialize a new instance of <see cref="FormContentModelMapper"/>
        /// </summary>
        public FormContentModelMapper(IContentTypeRepository contentTypeRepository,
                                      ReflectionService reflectionService,
                                      IContentModelReferenceConverter contentModelService,
                                      IPropertyConverterResolver propertyConverterResolver,
                                      IContentVersionRepository contentVersionRepository,
                                      ContentLoaderService contentLoaderService,
                                      IEPiServerFormsImplementationConfig formConfig,
                                      FormRenderingService formRenderingService,
                                      UrlResolverService urlResolverService,
                                      ContentApiConfiguration apiConfig)
            : base(contentTypeRepository,
                   reflectionService,
                   contentModelService,
                   contentVersionRepository,
                   contentLoaderService,
                   urlResolverService,
                   apiConfig,
                   propertyConverterResolver)
        {
            _apiConfiguration = apiConfig;
            _formConfig = formConfig;
            _formRenderingService = formRenderingService;
        }

        /// <inheritdoc />
        public override ContentApiModel Convert(IContent content, ConverterContext converterContext)
        {
            var model = base.Convert(content, converterContext) as FormContentApiModel;

            // When the form mapper is called from Search indexing job, the HttpContext will be null. In that case, we do not need to index the form model 
            // (because the form model is only needed when client calls the CD.Form endpoints to render the form in viewmode). 
            // So we should return the default model of cms content instead.
            if (HttpContext.Current == null)
            {
                return model;
            }

            var formModel = new FormContainerBlockModel()
            {
                Assets = new Dictionary<string, string>(),
                Template = _formRenderingService.BuildFormTemplate(content as IFormContainerBlock)
            };

            var inlineAssets = _formRenderingService.GetInlineAssets(content as FormContainerBlock, model.Language.Name);
            foreach (var asset in inlineAssets)
            {
                formModel.Assets.Add(asset.Key, asset.Value);
            }

            var bundledAssets = GetBundledAssets();
            foreach (var asset in bundledAssets)
            {
                formModel.Assets.Add(asset.Key, _formRenderingService.GetScriptFromAssemblyByName(asset.Value));
            }

            formModel.Assets.Add(FormInitScript, _formRenderingService.GetFormInitScript(content.ContentGuid, model.Language.Name));

            model.FormModel = formModel;
            return model;
        }

        /// <inheritdoc />
        protected override ContentApiModel CreateDefaultModel(IContent content)
        {
            var parent = _contentLoaderService.Get(content.ParentLink, (content as ILocalizable)?.Language?.Name);

            return new FormContentApiModel
            {
                ContentLink = _contentModelService.GetContentModelReference(content),
                Name = content.Name,
                ParentLink = _contentModelService.GetContentModelReference(parent)
            };
        }

        /// <summary>
        /// Get bundled script for Form
        /// </summary>
        protected virtual IDictionary<string, string> GetBundledAssets()
        {
            var bundledScriptsDict = new Dictionary<string, string>()
            {
                { "ViewModeJs", EPiServerFrameworkSection.Instance.ClientResources.Debug ? ConstantsForms.StaticResource.JS.EPiServerFormsPath : ConstantsForms.StaticResource.JS.EPiServerFormsMinifyPath}
            };

            if (_formConfig.InjectFormOwnJQuery)
            {
                bundledScriptsDict.Add("Jquery", ConstantsForms.StaticResource.JS.FormsjQueryPath);
            }

            if (_formConfig.InjectFormOwnStylesheet)
            {
                bundledScriptsDict.Add("Css", ConstantsForms.StaticResource.Css.EPiServerFormsPath);
            }

            return bundledScriptsDict;
        }
    }
}
