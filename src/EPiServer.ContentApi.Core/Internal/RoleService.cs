using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Security;
using System.Collections.Generic;

namespace EPiServer.ContentApi.Core.Internal
{
    /// <summary>
    /// Responsible for manipulating with role
    /// </summary>
    [ServiceConfiguration(typeof(RoleService))]
    public class RoleService
    {
        protected readonly IVirtualRoleRepository _virtualRoleRepository;

        public RoleService(IVirtualRoleRepository virtualRoleRepository)
        {
            _virtualRoleRepository = virtualRoleRepository;
        }

        /// <summary>
        /// Get mapped roles associated with a given virtual role
        /// </summary>
        public virtual IEnumerable<string> GetMappedRolesAssociatedWithVirtualRole(string virtualRole)
        {
            VirtualRoleProviderBase roleBase;
            _virtualRoleRepository.TryGetRole(virtualRole, out roleBase);

            if (roleBase == null || !(roleBase is MappedRole))
            {
                return null;
            }

            return (roleBase as MappedRole).Roles;
        }
    }
}
