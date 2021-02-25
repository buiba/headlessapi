using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Forms.Internal;
using EPiServer.Core;
using EPiServer.Forms;
using EPiServer.Forms.Controllers;
using EPiServer.Forms.Core;
using EPiServer.Forms.Helpers.Internal;
using EPiServer.Forms.Implementation.Elements;
using EPiServer.Framework;
using EPiServer.Framework.Cache;
using EPiServer.Framework.Web.Resources;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace EPiServer.ContentApi.Forms
{
    /// <summary>
    /// Service for rendering form template
    /// </summary>
    [ServiceConfiguration(typeof(FormRenderingService))]
    public class FormRenderingService
    {
        private const string _antiForgeryCookieKey = "__RequestVerificationToken";
        private const string _formsAssemblyName = "EPiServer.Forms";
        private const string _cacheKeyPrefix = "_EPiServer.Forms_ClientResource_";

        private readonly ISynchronizedObjectInstanceCache _cache;
        private readonly CommonUtility _commonUtility;
        private readonly IUrlResolver _urlResolver;
        private readonly FormResourceService _formResourceService;

        /// <summary>
        ///     Provide methods to get clent resources
        /// </summary>
        protected readonly IRequiredClientResourceList _requiredClientResourceList;

        /// <summary>
        /// to handle current principal
        /// </summary>
        protected readonly ISecurityPrincipal _securityPrincipal;

        /// <summary>
        ///     Initialzie a new instance of <see cref="FormRenderingService"/>
        /// </summary>
        public FormRenderingService(IRequiredClientResourceList requiredClientResourceList, 
            ISynchronizedObjectInstanceCache cache, 
            CommonUtility commonUtility,
            IUrlResolver urlResolver,
            ISecurityPrincipal securityPrincipal,
            FormResourceService formResourceService)
        {
            _requiredClientResourceList = requiredClientResourceList;
            _cache = cache;
            _commonUtility = commonUtility;
            _urlResolver = urlResolver;
            _securityPrincipal = securityPrincipal;
            _formResourceService = formResourceService;
        }

        /// <summary>
        ///     Initialzie a new instance of <see cref="FormRenderingService"/>
        /// </summary>
        public FormRenderingService(IRequiredClientResourceList requiredClientResourceList,
            ISynchronizedObjectInstanceCache cache,
            CommonUtility commonUtility,
            IUrlResolver urlResolver,
            ISecurityPrincipal securityPrincipal) : this (
                requiredClientResourceList,
                cache,
                commonUtility,
                urlResolver,
                securityPrincipal,
                ServiceLocator.Current.GetInstance<FormResourceService>())
        {            
        }

        /// <summary>
        /// Build form HTML template from given formContainerBlock.
        /// </summary>
        public virtual string BuildFormTemplate(IFormContainerBlock formContainerBlock)
        {
            var controller = new FormContainerBlockController();
            InitializeContext(controller);
            var actionResult = controller.Index(formContainerBlock as FormContainerBlock);

            return ExecuteActionResult(actionResult, controller);
        }

        /// <summary>
        /// Get the client-side init script of given form
        /// </summary>        
        public virtual string GetFormInitScript(Guid formGuid, string language)
        {
            var dataSubmitController = new DataSubmitController();
            InitializeContext(dataSubmitController);
            var actionResult = dataSubmitController.GetFormInitScript(formGuid, language);

            return ExecuteActionResult(actionResult, dataSubmitController);
        }

        /// <summary>
        /// Initialize the controller context from given controller and current HttpContext.
        /// </summary>
        protected virtual void InitializeContext(Controller controller)
        {
            // Instantiate a new RouteData, add controllerName param as controller route key
            var routeData = new System.Web.Routing.RouteData();
            routeData.Values.Add("controller", controller.GetType().Name);

            controller.ControllerContext = new ControllerContext(new HttpContextWrapper(HttpContext.Current), routeData, controller);
        }

        /// <summary>
        /// Execute an <see cref="ActionResult" /> with given controller and return its result
        /// </summary>
        /// <param name="actionResult">an action result to be executed</param>
        /// <param name="controller">controller used in combination with action result parameter</param>
        /// <returns></returns>
        protected virtual string ExecuteActionResult(ActionResult actionResult, Controller controller)
        {
            using (var writer = new StringWriter())
            {
                var response = new HttpResponse(writer);
                var tempContext = new HttpContext(HttpContext.Current.Request, response);
                var antiForgeryCookie = HttpContext.Current.Request.Cookies[_antiForgeryCookieKey];

                controller.ControllerContext.HttpContext = new HttpContextWrapper(tempContext);
                controller.ControllerContext.HttpContext.User = _securityPrincipal.GetCurrentPrincipal();

                actionResult.ExecuteResult(controller.ControllerContext);

                if (antiForgeryCookie == null)
                {
                    // Because Forms uses standard AntiForgeryToken of ASP.NET, set antiForgeryCookie in order to submit the form
                    HttpContext.Current.Response.SetCookie(controller.ControllerContext.HttpContext.Response.Cookies[_antiForgeryCookieKey]);
                }

                writer.Flush();

                return writer.ToString();
            }
        }

        /// <summary>
        /// Get inline script for a given form
        /// </summary>
        public virtual IDictionary<string, string> GetInlineAssets(FormContainerBlock formContainerBlock, string language)
        {
            var formController = new FormContainerBlockController();

            List<string> scripts, css;
            List<string> elementExtraScripts, elementExtraCss;
            _formResourceService.GetFormExternalResources(out scripts, out css);
            _formResourceService.GetFormElementExtraResources(formContainerBlock, out elementExtraScripts, out elementExtraCss);

            scripts.AddRange(elementExtraScripts);
            css.AddRange(elementExtraCss);

            language = language ?? FormsExtensions.GetCurrentFormLanguage(formContainerBlock);

            var prerequisiteScript = formController.GetPrerequisiteScript(scripts, css, language);
            var originalJQuery = formController.GetOriginalJQuery();

            return new Dictionary<string, string>()
            {
                { "OriginalJquery", originalJQuery },
                { "Prerequisite", prerequisiteScript }
            };
        }

        /// <summary>
        ///      Get the specified manifest script resource from EPiServer.Forms assembly.
        ///      Return empty string if not found.
        /// </summary>
        /// <param name="name">script name</param>
        public virtual string GetScriptFromAssemblyByName(string name)
        {
            Validator.ThrowIfNullOrEmpty(nameof(name), name);

            // load from cache first
            var cacheKey = $"{_cacheKeyPrefix}{name}";
            var scriptContent = _cache.Get(cacheKey) as string;
            if (!string.IsNullOrWhiteSpace(scriptContent))
            {
                return scriptContent;
            }

            scriptContent = _commonUtility.LoadResourceFromAssemblyByName(name, _formsAssemblyName);

            if (!string.IsNullOrWhiteSpace(scriptContent))
            {
                _cache.Insert(cacheKey, scriptContent, new CacheEvictionPolicy(TimeSpan.FromMinutes(int.MaxValue), CacheTimeoutType.Absolute));
            }

            return scriptContent ?? string.Empty;
        }

        /// <summary>
        /// Extract form's hosted page from current request       
        /// </summary>        
        public virtual PageReference ExtractCurrentPage()
        {            
            var currentPageUrl = HttpContext.Current.Request.Params["currentPageUrl"]?.ToString();
            if (!string.IsNullOrWhiteSpace(currentPageUrl))
            {
                var url = new UrlBuilder(currentPageUrl);
                var content = _urlResolver.Route(url, ContextMode.Default);

                if (content != null)
                {
                    return new PageReference(content.ContentLink);
                }
            }

            return null;
        }
    }
}
