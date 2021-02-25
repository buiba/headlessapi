using EPiServer.Core.Html.StringParsing;
using System;
using System.Collections.Generic;

namespace EPiServer.ContentApi.Core.Tests.Serialization
{
	[Serializable]
	internal class ConstFragment : IStringFragment
	{
		private String _string;
		public ConstFragment(string str)
		{
			_string = str;
		}

		#region IStringFragment Members

		public string InternalFormat
		{
			get { return _string.ToString(); }
		}

		public string GetEditFormat()
		{
			return InternalFormat;
		}

		public string GetViewFormat()
		{
			return InternalFormat;
		}

		#endregion

		#region IReferenceMap Members

		public IList<Guid> ReferencedPermanentLinkIds
		{
			get { return new List<Guid>(); }
		}

		public void RemapPermanentLinkReferences(IDictionary<Guid, Guid> idMap)
		{
		}

		#endregion
	}
}
