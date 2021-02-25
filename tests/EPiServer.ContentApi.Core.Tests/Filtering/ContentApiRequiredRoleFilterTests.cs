using System;
using System.Collections.Generic;
using System.Web;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.Core;
using EPiServer.Security;
using Xunit;
using Moq;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.Security.Internal;

namespace EPiServer.ContentApi.Core.Tests.Filtering
{
	public class ContentApiRequiredRoleFilterTests
	{
        private readonly Mock<RoleService> _roleService;
        private ContentApiRequiredRoleFilter requiredRoleFilter;
        private ContentApiConfiguration _apiConfig;

        public ContentApiRequiredRoleFilterTests()
        {
            _roleService = new Mock<RoleService>(null);
            _roleService.Setup(rs => rs.GetMappedRolesAssociatedWithVirtualRole(It.IsAny<string>()))
                        .Returns((string role) => { return null; });

            _apiConfig = new ContentApiConfiguration();
            _apiConfig.Default().SetRequiredRole("Content Api Access");

            requiredRoleFilter = CreateMockFilter(_roleService.Object);
        }

        [Fact]
		public void ShouldFilterContent_ShouldThrowArgumentException_WhenContentIsNull()
		{
            Assert.Throws<ArgumentException>(() => requiredRoleFilter.ShouldFilterContent(null));
		}

        /// <summary>
        /// If no required role is set up, then return false and no filter data
        /// </summary>
		[Fact]
		public void ShouldFilterContent_ShouldReturnFalse_WhenNoRequiredRole()
		{
            _apiConfig.Default().SetRequiredRole(string.Empty);
            requiredRoleFilter = CreateMockFilter(_roleService.Object);

            Assert.False(requiredRoleFilter.ShouldFilterContent(new PageData()));
		}

		[Fact]
		public void ShouldFilterContent_ShouldReturnFalse_WhenContentNotSecurable()
		{
			Assert.False(requiredRoleFilter.ShouldFilterContent(new BasicContent()));
		}

        [Fact]
		public void ShouldFilterContent_ShouldReturnTrue_WhenContentLacksRequiredRole()
		{
            _apiConfig.Default().SetRequiredRole("Content Api Access");
            var accessControlEntry = new AccessControlEntry("Everyone", AccessLevel.Read);

			var accessControlList = new ContentAccessControlList
			{
				accessControlEntry
			};

            Assert.True(requiredRoleFilter.ShouldFilterContent(new PageData(accessControlList, new PropertyDataCollection())));
		}

		[Fact]
		public void FilterContents_ShouldFilterContent_WhenNull()
		{
            var content = new List<IContent>
			{
				null
			};

			Assert.Empty(requiredRoleFilter.FilterContents(content));
		}

		[Fact]
		public void FilterContents_ShouldFilterContent_WhenMissingRole()
		{
            var accessControlEntry = new AccessControlEntry("Content Api Access", AccessLevel.Read);
			var accessControlList = new ContentAccessControlList
			{
				accessControlEntry
			};

            var content = new List<IContent>
			{
				new PageData(accessControlList, new PropertyDataCollection()),
				new PageData()
			};
            _roleService.Setup(rs => rs.GetMappedRolesAssociatedWithVirtualRole(It.IsAny<string>()))
                        .Returns((string role) => { return null; });            

            Assert.Single(requiredRoleFilter.FilterContents(content));
		}		

        private ContentApiRequiredRoleFilter CreateMockFilter(RoleService roleService = null)
        {
            return new ContentApiRequiredRoleFilter(roleService, _apiConfig);
        }
	}
}
