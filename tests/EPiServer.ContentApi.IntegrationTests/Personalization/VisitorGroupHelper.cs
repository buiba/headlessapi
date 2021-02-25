using EPiServer.Personalization.VisitorGroups;
using EPiServer.ServiceLocation;
using System;

namespace EPiServer.ContentApi.IntegrationTests.Personalization
{
    public class VisitorGroupHelper
    {
        public static VisitorGroup GenerateVisitorGroup()
        {
            var visitorGroup = new VisitorGroup
            {
                Name = Guid.NewGuid().ToString(),
                Criteria = {
                    new VisitorGroupCriterion
                    {
                        Required = true,
                        Model = new TestVisitorModel(),
                        TypeName = typeof(TestVisitorCriterion).FullName
                    }
                }
            };

            var repository = ServiceLocator.Current.GetInstance<IVisitorGroupRepository>();
            repository.Save(visitorGroup);

            return visitorGroup;
        }
    }
}
