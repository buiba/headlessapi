using EPiServer.ContentApi.Core.Security;
using EPiServer.ContentApi.Forms.Internal;
using EPiServer.Core;
using EPiServer.Forms;
using EPiServer.Framework.Cache;
using EPiServer.Framework.Web.Resources;
using EPiServer.Security;
using EPiServer.Web;
using Moq;
using System;
using System.Collections.Generic;
using System.Web;
using Xunit;

namespace EPiServer.ContentApi.Forms.Tests
{
    public class FormRenderingServiceTest : TestBase
    {
        protected FormRenderingService Subject;
        protected readonly Mock<IRequiredClientResourceList> _requiredClientResourceList;
        protected readonly Mock<ISynchronizedObjectInstanceCache> _objectInstanceCache;
        protected readonly Mock<CommonUtility> _commonUtility;        

        public FormRenderingServiceTest()
        {
            _requiredClientResourceList = new Mock<IRequiredClientResourceList>();
            _objectInstanceCache = new Mock<ISynchronizedObjectInstanceCache>();
            _commonUtility = new Mock<CommonUtility>();            

            var clientResources = new List<ClientResource>();
            clientResources.Add(new ClientResource
            {
                Name = ConstantsForms.StaticResource.JS.EPiServerFormsPrerequisite,
                InlineContent = "myScriptContent",
                Dependencies = new List<string>()
            });
            clientResources.Add(new ClientResource
            {
                Name = ConstantsForms.StaticResource.JS.EPiServerFormsPath,
                InlineContent = "myScriptContent",
                Dependencies = new List<string>()
            });

            _requiredClientResourceList.Setup(x => x.GetClientResources()).Returns(() => clientResources);

            Subject = new FormRenderingService(_requiredClientResourceList.Object, _objectInstanceCache.Object, _commonUtility.Object, _urlResolver.Object, new Mock<ISecurityPrincipal>().Object, _formResourceService.Object);            
        }
      

        [Fact]
        public void GetScriptFromAssemblyByName_WhenScriptNameIsNull_ShouldTHrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Subject.GetScriptFromAssemblyByName(null));
        }

        [Fact]
        public void GetScriptFromAssemblyByName_WhenScriptNameIsEmpty_ShouldTHrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Subject.GetScriptFromAssemblyByName(string.Empty));
        }

        [Fact]
        public void GetScriptFromAssemblyByName_WhenScriptIsNotExistedInAssembly_ShouldReturnEmpty()
        {
            var scriptContent = Subject.GetScriptFromAssemblyByName("myScriptName.js");
            Assert.Empty(scriptContent);
        }

        [Fact]
        public void GetScriptFromAssemblyByName_WhenScriptIsExistedInAssembly_ShouldReturnContent()
        {
            _commonUtility.Setup(x => x.LoadResourceFromAssemblyByName(It.IsAny<string>(), It.IsAny<string>())).Returns("scriptContent");
            var scriptContent = Subject.GetScriptFromAssemblyByName(ConstantsForms.StaticResource.JS.EPiServerFormsPath);
            Assert.NotEmpty(scriptContent);
        }

        [Fact]
        public void GetScriptFromAssemblyByName_WhenScriptIsInCache_ShouldReturnFromCache()
        {
            _objectInstanceCache.Setup(c => c.Get(It.IsAny<string>())).Returns("scriptContent");

            var scriptContent = Subject.GetScriptFromAssemblyByName(ConstantsForms.StaticResource.JS.EPiServerFormsPrerequisite);

            Assert.NotEmpty(scriptContent);
            // not loaded from Assembly.Load()
            _commonUtility.Verify(x => x.LoadResourceFromAssemblyByName(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetScriptFromAssemblyByName_WhenScriptIsNotInCache_ShouldReturnFromAssembly()
        {
            _objectInstanceCache.Setup(c => c.Get(It.IsAny<string>())).Returns(null);
            _commonUtility.Setup(x => x.LoadResourceFromAssemblyByName(It.IsAny<string>(), It.IsAny<string>())).Returns("scriptContent");

            var scriptContent = Subject.GetScriptFromAssemblyByName(ConstantsForms.StaticResource.JS.EPiServerFormsPrerequisite);

            Assert.Equal("scriptContent", scriptContent);
            // not loaded from Assembly.Load()
            _commonUtility.Verify(x => x.LoadResourceFromAssemblyByName(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ExtractHostedPage_WhenMissingCurrentPageParam_ShouldReturnNull()
        {            
            HttpContext.Current = CreateHttpContext("http://newhttpcontext.com", string.Empty);

            var pageReference = Subject.ExtractCurrentPage();

            Assert.Null(pageReference);            
        }

        [Fact]
        public void ExtractHostedPage_WhenCurrentPageParamExisted_ShouldReturnPageReference()
        {
            var properties = new PropertyDataCollection();
            properties.Add("PageLink", new PropertyPageReference(new PageReference(5, 5)));
            var currentPage = new PageData(new AccessControlList(), properties);

            HttpContext.Current = CreateHttpContext("http://newhttpcontext.com", "currentPageUrl=en/page1");
            _urlResolver.Setup(x => x.Route(It.IsAny<UrlBuilder>(), It.IsAny<ContextMode>())).Returns(currentPage);            

            var pageReference = Subject.ExtractCurrentPage();

            Assert.True(pageReference.Equals(currentPage.ContentLink));
        }        
    }
}
