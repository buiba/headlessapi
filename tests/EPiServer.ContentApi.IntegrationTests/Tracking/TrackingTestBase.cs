using EPiServer.ContentApi.Core.Tracking;
using EPiServer.ContentApi.IntegrationTests.ContentModels.Blocks;
using EPiServer.ContentApi.IntegrationTests.TestSetup;
using EPiServer.Core;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Personalization.VisitorGroups;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.ContentApi.IntegrationTests
{
    public class TrackingTestBase
    {
        protected const string V2Uri = "api/episerver/v2.0/content";        
        protected ServiceFixture _fixture;
        protected VisitorGroup _visitorGroup;

        public TrackingTestBase(ServiceFixture fixture)
        {
            _fixture = fixture;
            _visitorGroup = Personalization.VisitorGroupHelper.GenerateVisitorGroup();
        }

        protected ContentArea CreateContentArea(bool createWithPersonalizeContentItem)
        {
            var block = _fixture.GetWithDefaultName<TextBlock>(SiteDefinition.Current.GlobalAssetsRoot, true);
            var contentArea = new ContentArea();
            contentArea.Items.Add(new ContentAreaItem
            {
                ContentLink = (block as IContent).ContentLink,
                ContentGuid = (block as IContent).ContentGuid,
                AllowedRoles = createWithPersonalizeContentItem ? new string[] { _visitorGroup.Id.ToString() } : null
            });

            return contentArea;
        }

        protected XhtmlString CreatePersonalizedXhtmlString()
        {
            var xhtmlString = new XhtmlString();
            var personalizedContentFactory = ServiceLocator.Current.GetInstance<EPiServer.Personalization.IPersonalizedContentFactory>();
            var securedMarkupGeneratorFactory = ServiceLocator.Current.GetInstance<ISecuredFragmentMarkupGeneratorFactory>();
            var securedMarkupGenerator = securedMarkupGeneratorFactory.CreateSecuredFragmentMarkupGenerator();
            securedMarkupGenerator.RoleSecurityDescriptor.RoleIdentities = new[] { _visitorGroup.Id.ToString() };

            var fragment = new PersonalizedContentFragment(personalizedContentFactory, securedMarkupGenerator)
            {
                Fragments = { new StaticFragment("Personalized fragment") }
            };

            xhtmlString.Fragments.Add(fragment);

            return xhtmlString;
        }

        protected IEnumerable<string> GetAllPersonalizedProperties(ContentApiTrackingContext trackingContext)
        {
            return trackingContext.ReferencedContent.Values.Where(rf => rf != null).SelectMany(rf => rf.PersonalizedProperties);
        }
    }
}
