using EPiServer.Core.Html.StringParsing;

namespace EPiServer.ContentApi.Core.Tests.Serialization
{
	internal class MockableConstFragmentParser : IFragmentParser
	{
		private bool _securable;

		public MockableConstFragmentParser() : this(false)
		{
		}

		public MockableConstFragmentParser(bool securable)
		{
			_securable = securable;
		}

		public StringFragmentCollection Parse(string content, FragmentParserMode parserMode, bool evaluateHash)
		{
			var fragCollection = new StringFragmentCollection();
			fragCollection.Add(new ConstFragment(content));
			return fragCollection;
		}
	}
}
