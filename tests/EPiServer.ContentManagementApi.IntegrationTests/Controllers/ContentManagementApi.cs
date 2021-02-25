using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Pages;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.ContentManagementApi.Controllers;
using EPiServer.ContentManagementApi.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Personalization.VisitorGroups;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Xunit;
using VisitorGroupHelper = EPiServer.ContentManagementApi.IntegrationTests.Personalization.VisitorGroupHelper;

namespace EPiServer.ContentManagementApi.IntegrationTests.Controllers
{
    [Collection(IntegrationTestCollection.Name)]
    public partial class ContentManagementApi : IAsyncLifetime
    {
        private readonly ServiceFixture _fixture;
        private const string AuthorizedRole = "Authorized";
        private readonly string _anonymousRole = "Anonymous";
        private readonly string _contentApiWriteRole = "contentapiwrite";
        private const string ContentGuid = "B343613A-7119-499A-BCC7-6475D12F3912";
        private IContent _publicContent;
        private IContent _deletedContent;
        private IContent _securedContent;
        private IContent _securedFolder;
        private IContent _securedContentWithNonBranchSpecific;
        private IContent _securedContentWithNestedBlock;
        private IContent _securedContentWithNonBranchSpecificNestedBlock;
        private IContent _securedContentWithSpecifiedGuid;
        private IContent _securedFile;
        private Category _rootCategory;
        private readonly VisitorGroup _visitorGroup;
        private readonly ContentManagementRepository _contentManagementRepository;
        private readonly IContentSecurityRepository _contentAccessRepository;
        private readonly IContentVersionRepository _contentVersionRepository;
        private readonly IContentRepository _contentRepository;

        public ContentManagementApi(ServiceFixture fixture)
        {
            _fixture = fixture;
            _visitorGroup = VisitorGroupHelper.GenerateVisitorGroup();
            _contentManagementRepository = ServiceLocator.Current.GetInstance<ContentManagementRepository>();
            _contentAccessRepository = ServiceLocator.Current.GetInstance<IContentSecurityRepository>();
            _contentVersionRepository = ServiceLocator.Current.GetInstance<IContentVersionRepository>();
            _contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        }

        public Task InitializeAsync()
        {
            _publicContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "en");
            SetupSecurityDescriptor(_publicContent.ContentLink);

            _deletedContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "en");
            SetupSecurityDescriptor(_deletedContent.ContentLink);

            // Create source page for moving endpoint.
            _sourceContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "en");
            SetupSecurityDescriptor(_sourceContent.ContentLink);

            // Create destination page moving endpoint.
            _destinationContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "en");
            SetupSecurityDescriptor(_destinationContent.ContentLink);

            _securedContent = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "en");
            var securedDescriptor = _contentAccessRepository.Get(_securedContent.ContentLink).CreateWritableClone() as IContentSecurityDescriptor;
            securedDescriptor.ToLocal(true);
            securedDescriptor.RemoveEntry(securedDescriptor.Entries.Single(e => e.Name.Equals(EveryoneRole.RoleName)));
            securedDescriptor.AddEntry(new AccessControlEntry(AuthorizedRole, AccessLevel.FullAccess,
                SecurityEntityType.Role));
            _contentAccessRepository.Save(_securedContent.ContentLink, securedDescriptor, SecuritySaveType.Replace);

            _securedContentWithNonBranchSpecific = _fixture.GetWithDefaultName<StandardPageWithNonBranchSpecificProperty>(ContentReference.StartPage, true, "en");
            SetupSecurityDescriptor(_securedContentWithNonBranchSpecific.ContentLink);

            _securedContentWithNestedBlock = _fixture.GetWithDefaultName<StandardPageWithNestedBlock>(ContentReference.StartPage, true, "en");
            SetupSecurityDescriptor(_securedContentWithNestedBlock.ContentLink);

            _securedContentWithNonBranchSpecificNestedBlock = _fixture.GetWithDefaultName<StandardPageWithNonBranchSpecificNestedBlock>(ContentReference.StartPage, true, "en");
            SetupSecurityDescriptor(_securedContentWithNonBranchSpecificNestedBlock.ContentLink);

            _securedContentWithSpecifiedGuid = _fixture.GetWithDefaultName<StandardPage>(ContentReference.StartPage, true, "en", (c) => { (c as IContent).ContentGuid = new Guid(ContentGuid); });
            SetupSecurityDescriptor(_securedContentWithSpecifiedGuid.ContentLink);

            // Add access level for content folder
            _securedFolder = _fixture.GetWithDefaultName<ContentFolder>(ContentReference.GlobalBlockFolder, true, "en");
            SetupSecurityDescriptor(_securedFolder.ContentLink);

            // Add access level for media
            _securedFile = _fixture.GetWithDefaultName<GenericFile>(ContentReference.GlobalBlockFolder, true, "en");
            SetupSecurityDescriptor(_securedFile.ContentLink);

            // Create root category
            _rootCategory = _fixture.CategoryRepository.GetRoot();

            return Task.CompletedTask;
        }

        private void SetupSecurityDescriptor(ContentReference contentLink, bool addContentApiWriteRole = false)
        {
            var securityDescriptor = _contentAccessRepository.Get(contentLink).CreateWritableClone() as IContentSecurityDescriptor;
            if (securityDescriptor.IsInherited)
            {
                securityDescriptor.ToLocal(true);
            }

            securityDescriptor.AddEntry(new AccessControlEntry(AuthorizedRole, AccessLevel.FullAccess, SecurityEntityType.Role));
            securityDescriptor.AddEntry(new AccessControlEntry(_anonymousRole, AccessLevel.NoAccess, SecurityEntityType.VisitorGroup));
            if (addContentApiWriteRole)
            {
                securityDescriptor.AddEntry(new AccessControlEntry(_contentApiWriteRole, AccessLevel.Read, SecurityEntityType.Role));
            }

            _contentAccessRepository.Save(contentLink, securityDescriptor, SecuritySaveType.Replace);
        }

        private void CleanupContent(Guid contentGuid)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, ContentManagementApiController.RoutePrefix + contentGuid);
            requestMessage.Headers.Add(HeaderConstants.PermanentDeleteHeaderName, "true");
            _fixture.Client.SendAsync(requestMessage);
        }

        public Task DisposeAsync()
        {
            _fixture.ContentRepository.Delete(_securedContent.ContentLink, true, AccessLevel.NoAccess);
            _fixture.ContentRepository.Delete(_publicContent.ContentLink, true, AccessLevel.NoAccess);
            _fixture.ContentRepository.Delete(_securedContentWithSpecifiedGuid.ContentLink, true, AccessLevel.NoAccess);
            return Task.CompletedTask;
        }
    }
}
