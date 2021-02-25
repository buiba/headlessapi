using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using EPiServer.Security;
using Moq;
using Xunit;

namespace EPiServer.ContentApi.Core.Tests.Filtering
{
    public class ContentApiRoleServiceTest : TestBase
    {  
        [Fact]
        public void GetAssociatedMappedRolesWithVirtualRole_ShouldReturnNull_WhenVirtualRoleIsNull()
        {
            Assert.Null(_contentApiRoleService.GetMappedRolesAssociatedWithVirtualRole(null));
        }

        [Fact]
        public void GetAssociatedMappedRolesWithVirtualRole_ShouldReturnNull_WhenVirtualRoleIsNotMappedRole()
        {
            var providerBase = new CustomizeRole() as VirtualRoleProviderBase;
            _virtualRoleRepository.Setup(repo => repo.TryGetRole(It.IsAny<string>(), out providerBase));

            Assert.Null(_contentApiRoleService.GetMappedRolesAssociatedWithVirtualRole("yahoo"));
        }

        [Fact]
        public void GetAssociatedMappedRolesWithVirtualRole_ShouldReturnRoles_WhenVirtualRoleIsMappedRole()
        {
            var providerBase = new MappedRole() { Roles = new List<string>() { "Role 1", "Role 2" } } as VirtualRoleProviderBase;
            _virtualRoleRepository.Setup(repo => repo.TryGetRole(It.IsAny<string>(), out providerBase));

            var actual = _contentApiRoleService.GetMappedRolesAssociatedWithVirtualRole("Role");
            var expected = (providerBase as MappedRole).Roles;
            Assert.True(actual.SequenceEqual(expected));
        }
    }

    public class CustomizeRole : VirtualRoleProviderBase
    {
        public override bool IsInVirtualRole(IPrincipal principal, object context)
        {
            throw new NotImplementedException();
        }
    }
}
