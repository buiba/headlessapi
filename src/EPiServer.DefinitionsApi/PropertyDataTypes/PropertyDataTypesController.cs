using System;
using System.Diagnostics.CodeAnalysis;
using System.Web.Http;
using System.Web.Http.Description;
using EPiServer.ContentApi.Cors.Internal;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.DefinitionsApi.Internal;
using EPiServer.DefinitionsApi.PropertyDataTypes.Internal;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;

namespace EPiServer.DefinitionsApi.PropertyDataTypes
{
    /// <summary>
    /// REST Controller for listing property data types.
    /// </summary>
    [Error]
    [Cors]
    public class PropertyDataTypesController : ApiController
    {
        internal const string RoutePrefix = ApiRoute.Prefix + "propertydatatypes/";
        private readonly PropertyDataTypeResolver _resolver;

        // Default constructor is required as we don't want to require that applications implement
        // a DependencyResolver

        /// <exclude />
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1604:Element documentation should have summary", Justification = "Member is excluded")]
        public PropertyDataTypesController()
            : this(ServiceLocator.Current.GetInstance<PropertyDataTypeResolver>())
        { }

        internal PropertyDataTypesController(PropertyDataTypeResolver resolver)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        /// <summary>
        /// List all property data types available in the system.
        /// </summary>
        /// <returns>A list of all property data types.</returns>
        /// <response code="200">Ok</response>
        [Route(RoutePrefix)]
        [HttpGet]
        [ResponseType(typeof(ExternalPropertyDataType[]))]
        public IHttpActionResult List() => Json(_resolver.List(), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
    }
}
