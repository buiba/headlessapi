using System.Collections.Generic;
using System.Linq;
using EPiServer.Security;

namespace EPiServer.ContentApi.IntegrationTests.TestSetup
{
    internal class NoopSecurityEntityProvider : SecurityEntityProvider
    {
        public override IEnumerable<string> GetRolesForUser(string userName) => Enumerable.Empty<string>();

        public override IEnumerable<SecurityEntity> Search(string partOfValue, string claimType) => Enumerable.Empty<SecurityEntity>();


        public override IEnumerable<SecurityEntity> Search(string partOfValue, string claimType, int startIndex, int maxRows, out int totalCount)
        {
            totalCount = 0;
            return Enumerable.Empty<SecurityEntity>();
        }
    }
}
