using System;
using EPiServer.ContentApi.Core.Internal;
using EPiServer.ContentManagementApi.Configuration;
using EPiServer.Core;
using EPiServer.Security;
using Moq;
using Xunit;

namespace EPiServer.ContentManagementApi.Internal
{
    public class RequiredRoleEvaluatorTest
    {
        private readonly Mock<RoleService> _mockRoleService;
        private readonly ContentManagementApiOptions _options;

        public RequiredRoleEvaluatorTest()
        {
            _mockRoleService = new Mock<RoleService>(null);
            _options = new ContentManagementApiOptions();
        }

        private protected RequiredRoleEvaluator Subject()
        {
            return new RequiredRoleEvaluator(_mockRoleService.Object, _options);
        }

        [Fact]
        public void HasAccess_WhenContentIsNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Subject().HasAccess(null));
        }

        [Fact]
        public void HasAccess_WhenRequiredRoleIsNotSet_ShouldReturnTrue()
        {
            _options.RequiredRole = null;
            var result = Subject().HasAccess(new PageData());
            Assert.True(result);
        }

        [Fact]
        public void HasAccess_WhenAccessControlEntriesIsNull_ShouldReturnTrue()
        {
            var content = new PageData() { ACL = new AccessControlList() };
            var result = Subject().HasAccess(content);
            Assert.True(result);
        }

        [Fact]
        public void HasAccess_WhenContentIsNotExposedToContentApiWriteRole_ShouldReturnFalse()
        {
            var aclList = new ContentAccessControlList
            {
                new AccessControlEntry("Administrator", AccessLevel.Read),
            };
            var content = new PageData(aclList, new PropertyDataCollection());

            var result = Subject().HasAccess(content);
            Assert.False(result);
        }

        [Fact]
        public void HasAccess_WhenContentIsExposedToContentApiWriteRole_ShouldReturnTrue()
        {
            var aclList = new ContentAccessControlList
            {
                new AccessControlEntry("contentapiwrite", AccessLevel.Read),
            };
            var content = new PageData(aclList, new PropertyDataCollection());

            var result = Subject().HasAccess(content);
            Assert.True(result);
        }

    }
}
