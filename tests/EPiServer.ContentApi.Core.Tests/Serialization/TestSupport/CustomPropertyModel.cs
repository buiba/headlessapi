using EPiServer.ContentApi.Core.Serialization.Models;

namespace EPiServer.ContentApi.Core.Tests.Serialization.TestSupport
{
	internal class CustomPropertyModel : PropertyModel<string, CustomPropertyData>
	{
		public CustomPropertyModel(CustomPropertyData type) : base(type)
		{
		}
	}
}
