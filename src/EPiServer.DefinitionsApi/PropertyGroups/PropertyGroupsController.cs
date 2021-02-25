using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using EPiServer.ContentApi.Cors.Internal;
using EPiServer.ContentApi.Error.Internal;
using EPiServer.ContentApi.Security.Internal;
using EPiServer.DefinitionsApi.Internal;
using EPiServer.DefinitionsApi.PropertyGroups.Internal;
using EPiServer.ServiceLocation;

namespace EPiServer.DefinitionsApi.PropertyGroups
{
    /// <summary>
    /// REST Controller for property group.
    /// </summary>
    [Error]
    [Cors]
    [ApiAuthorize]
    public class PropertyGroupsController : ApiController
    {
        internal const string RoutePrefix = ApiRoute.Prefix + "propertygroups/";
        private const string GetPropertyGroupRouteName = "GetPropertyGroup";
        private readonly PropertyGroupRepository _propertyGroupRepository;

        // Default constructor is required as we don't want to require that applications implement
        // a DependencyResolver
        public PropertyGroupsController() : this(ServiceLocator.Current.GetInstance<PropertyGroupRepository>())
        {
        }

        internal PropertyGroupsController(PropertyGroupRepository propertyGroupRepository)
        {
            _propertyGroupRepository = propertyGroupRepository ?? throw new ArgumentNullException(nameof(propertyGroupRepository));
        }

        /// <summary>
        /// Gets a property group with the provided name.
        /// </summary>
        /// <param name="name">The identifier of the property group.</param>
        /// <returns>The property group.</returns>
        /// <response code="200">Ok</response>
        /// <response code="404">Not found</response>
        [Route(RoutePrefix + "{name}", Name = GetPropertyGroupRouteName)]
        [HttpGet]
        [ResponseType(typeof(PropertyGroupModel))]
        public IHttpActionResult Get(string name)
        {
            var propertyGroup = _propertyGroupRepository.Get(name);

            if (propertyGroup is null)
            {
                return NotFound();
            }

            return Json(propertyGroup);
        }

        /// <summary>
        /// Deletes the property group with the provided name.
        /// </summary>
        /// <param name="name">The name of the property group.</param>
        /// <response code="204">No content</response>
        /// <response code="404">Not found</response>
        [Route(RoutePrefix + "{name}")]
        [HttpDelete]
        [ResponseType(typeof(void))]
        public IHttpActionResult Delete(string name)
        {
            if (_propertyGroupRepository.TryDelete(name))
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            return NotFound();
        }

        /// <summary>
        /// List all property groups in the system.
        /// </summary>
        /// <returns>A list of all property groups.</returns>
        /// <response code="200">Ok</response>
        [Route(RoutePrefix)]
        [HttpGet]
        [ResponseType(typeof(PropertyGroupModel[]))]
        public IHttpActionResult List()
        {
            var propertyGroups = _propertyGroupRepository.List();
            return Json(propertyGroups);
        }

        /// <summary>
        /// Creates a new property group in the system.
        /// </summary>
        /// <param name="propertyGroupModel">The property group that should be created.</param>
        /// <response code="201">Created</response>
        /// <response code="400">Bad request</response>
        /// <response code="409">Conflict</response>
        [Route(RoutePrefix)]
        [HttpPost]
        [ValidateModel]
        [ResponseType(typeof(PropertyGroupModel))]
        public IHttpActionResult Create([Required][FromBody] PropertyGroupModel propertyGroupModel)
        {
            if (propertyGroupModel is null)
            {
                return this.Problem(HttpStatusCode.BadRequest, $"propertyGroupModel cannot be null");
            }

            if (propertyGroupModel.SystemGroup.HasValue && propertyGroupModel.SystemGroup.Value)
            {
                return this.Problem(HttpStatusCode.BadRequest, $"Cannot create the system group");
            }

            var existing = _propertyGroupRepository.Get(propertyGroupModel.Name);
            if (existing is object)
            {
                return this.Problem(HttpStatusCode.Conflict, "There is already another property group with the provided name.");
            }

            _propertyGroupRepository.Create(propertyGroupModel);

            return CreatedAtRoute(GetPropertyGroupRouteName, new { name = propertyGroupModel.Name }, propertyGroupModel);
        }

        /// <summary>
        /// Updates or creates a property group in the system with the provided name.
        /// </summary>
        /// <param name="name">The name of the property group.</param>
        /// <param name="propertyGroupModel">The property group that should be updated.</param>
        /// <response code="200">Ok</response>
        /// <response code="201">Created</response>
        /// <response code="400">Bad request</response>
        /// <response code="409">Conflict</response>
        [Route(RoutePrefix + "{name}")]
        [HttpPut]
        [ValidateModel]
        [ResponseType(typeof(PropertyGroupModel))]
        public IHttpActionResult CreateOrUpdate(string name, [Required][FromBody] PropertyGroupModel propertyGroupModel)
        {
            if (propertyGroupModel is null)
            {
                return this.Problem(HttpStatusCode.BadRequest, $"propertyGroupModel cannot be null");
            }

            if (name != propertyGroupModel.Name)
            {
                return this.Problem(HttpStatusCode.BadRequest, "The name on the provided property group does not match the resource location and cannot be changed.");
            }

            var action = _propertyGroupRepository.Save(propertyGroupModel);
            if (action == SaveResult.Created)
            {
                return CreatedAtRoute(GetPropertyGroupRouteName, new { name = propertyGroupModel.Name }, propertyGroupModel);
            }
            else
            {
                return Json(propertyGroupModel);
            }
        }
    }
}
