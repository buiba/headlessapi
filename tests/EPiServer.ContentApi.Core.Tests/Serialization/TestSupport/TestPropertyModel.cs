using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.Core;

namespace EPiServer.ContentApi.Core.Tests.Serialization.TestSupport
{
    internal class TestPropertyModel : PropertyModel<string,PropertyString>
    {
        public TestPropertyModel(PropertyString type) : base(type)
        {
        }
    }
}
