using System.Security.Principal;
using System.Web;
using EPiServer.Personalization.VisitorGroups;

namespace EPiServer.ContentManagementApi.IntegrationTests.Personalization
{
    public class TestVisitorModel : CriterionModelBase
    {
        public override ICriterionModel Copy()
        {
            return new TestVisitorModel();
        }
    }

    [VisitorGroupCriterion(Category = "Test", DisplayName = "Test Criterion")]
    public class TestVisitorCriterion : CriterionBase<TestVisitorModel>
    {
        public override bool IsMatch(IPrincipal principal, HttpContextBase httpContext)
        {
            return false;
        }
    }
}
